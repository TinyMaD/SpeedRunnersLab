<template>
  <v-navigation-drawer
    v-model="drawer"
    temporary
    stateless
    width="500px"
    dark
    right
    absolute
    @input="onChange"
  >
    <v-container>
      <v-form ref="form" v-model="valid">
        <v-select
          required
          :items="items"
          label="类 型"
          :rules="[v => !!v || '请选择类型']"
        />
        <v-text-field
          v-model="fileName"
          :rules="nameRules"
          :counter="17"
          clearable
          required
          label="标 题"
        />
        <v-img max-width="250px" max-height="160px" :src="imgSrc" />
        <v-file-input
          accept="image/*"
          label="点击上传封面"
          @change="changeImg"
        />
        <v-progress-linear
          v-model="progress"
        />
        <v-file-input
          :show-size="true"
          accept="image/*"
          label="点击上传文件"
          @change="changeFiles"
        />
        <v-btn
          :disabled="!valid"
          color="success"
          class="mr-4"
          @click="uploadFile"
        >
          提 交
        </v-btn>
        <v-btn
          color="info"
          class="mr-4"
          @click="doClose"
        >
          取 消
        </v-btn>
      </v-form>
      <ImgCropper :visible.sync="showCropper" :src="cropperImgUrl" @done="getCroppedImg" />
    </v-container>
  </v-navigation-drawer>
</template>
<script>
import * as qiniu from "qiniu-js";
import { getUploadToken } from "@/api/asset";
import ImgCropper from "@/components/ImgCropper";
export default {
  components: {
    ImgCropper
  },
  props: {
    visible: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      items: [
        { text: "人 物", value: "1" },
        { text: "轨 迹", value: "2" },
        { text: "道 具", value: "3" },
        { text: "HUD", value: "4" },
        { text: "音 效", value: "5" },
        { text: "背 景", value: "6" },
        { text: "其 它", value: "7" }
      ],
      drawer: this.visible,
      showCropper: false,
      imgSrc: "",
      cropperImgUrl: "",
      file: {},
      subscription: null,
      progress: 0,
      valid: false,
      fileName: "",
      nameRules: [
        v => !!v || "请输入标题",
        v => !v || v.length <= 17 || "标题字数不能大于17"
      ]
    };
  },
  watch: {
    visible() {
      this.drawer = this.visible;
    }
  },
  methods: {
    changeImg(file) {
      if (file) {
        this.cropperImgUrl = URL.createObjectURL(file);
        this.showCropper = true;
      }
    },
    changeFiles(file) {
      this.file = file;
    },
    getCroppedImg(src) {
      this.imgSrc = src;
    },
    uploadFile() {
      var that = this;
      getUploadToken().then(response => {
        const token = response.data;
        // const putExtra = {
        //    fname: "qiniu.txt"
        //    customVars: { "x:test": "qiniu" },
        //    metadata: { "x-qn-meta": "qiniu" }
        // };
        const config = {
          useCdnDomain: true
        };
        const observable = qiniu.upload(that.file, that.file.name, token, null, config);
        const observer = {
          next(res) {
            that.progress = res.total.percent;
          },
          error(err) {
            that.$toast.error(`${err.name}:${err.message}`);
          },
          complete(res) {
            that.progress = 100;
            console.log(res);
          }
        };
        that.subscription = observable.subscribe(observer);
      });
    },
    doClose() {
      this.drawer = false;
      this.onChange();
    },
    onChange() {
      if (this.drawer === false) {
        this.$emit("update:visible", false);
        this.$refs.form.reset();
        this.imgSrc = "";
      }
    }
  }
};
</script>