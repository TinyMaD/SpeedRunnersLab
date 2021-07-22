<template>
  <v-navigation-drawer
    v-model="drawer"
    width="500px"
    dark
    right
    absolute
    temporary
    @input="$emit('update:visible', drawer)"
  >
    <v-container>
      <v-form v-model="valid">
        <v-text-field
          v-model="fileName"
          color="green"
          :rules="nameRules"
          :counter="17"
          clearable
          required
          label="标 题"
        />
        <v-btn>
          上传封面
        </v-btn>
        <v-progress-linear
          v-model="progress"
        />
        <v-file-input
          :show-size="true"
          accept="image/*"
          label="点击选择文件"
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
      </v-form>
    </v-container>
  </v-navigation-drawer>
</template>
<script>
import * as qiniu from "qiniu-js";
import { getUploadToken } from "@/api/asset";
export default {
  props: {
    visible: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      drawer: this.visible,
      file: {},
      subscription: null,
      progress: 0,
      valid: false,
      fileName: "",
      lastname: "",
      nameRules: [
        v => !!v || "请输入标题",
        v => v.length <= 17 || "标题字数不能大于17"
      ],
      email: "",
      emailRules: [
        v => !!v || "E-mail is required",
        v => /.+@.+/.test(v) || "E-mail must be valid"
      ]
    };
  },
  watch: {
    visible() {
      this.drawer = this.visible;
    }
  },
  methods: {
    changeFiles(file) {
      this.file = file;
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
    }
  }
};
</script>