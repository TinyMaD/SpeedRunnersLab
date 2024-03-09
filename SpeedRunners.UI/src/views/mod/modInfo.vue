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
      <v-form ref="form" v-model="valid" :disabled="uploading">
        <v-select
          v-model="formType"
          required
          :items="items"
          :label=" $t('common.type') "
          :rules="[v => v!==undefined || $t('common.selectType')]"
        />
        <v-text-field
          v-model="formTitle"
          :rules="nameRules"
          :counter="17"
          clearable
          required
          :label="$t('common.title')"
        />
        <v-img max-width="250px" max-height="160px" :src="imgSrc" />
        <v-progress-linear
          v-model="imgProgress"
        />
        <v-file-input
          ref="imgInput"
          accept="image/*"
          :label="$t('mod.uploadCover')"
          :rules="imgRules"
          prepend-icon="mdi-camera"
          @change="changeImg"
          @click="clearImg"
        />
        <v-progress-linear
          v-model="fileProgress"
        />
        <v-file-input
          ref="fileInput"
          :show-size="true"
          :accept="fileAccept"
          :label="$t('mod.uploadFile')"
          :rules="fileRules"
          @change="changeFiles"
          @click="clearFiles"
        />
        <v-btn
          :disabled="!valid||uploading"
          color="success"
          class="mr-4"
          @click="submitMod"
        >
          {{ $t('common.submit') }}
        </v-btn>
        <v-btn
          :disabled="uploading"
          color="info"
          class="mr-4"
          @click="doClose"
        >
          {{ $t('common.cancel') }}
        </v-btn>
      </v-form>
      <ImgCropper :visible.sync="showCropper" :src="cropperImgUrl" @done="getCroppedImg" />
    </v-container>
  </v-navigation-drawer>
</template>
<script>
import * as qiniu from "qiniu-js";
import { getUploadToken, addMod } from "@/api/asset";
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
      drawer: this.visible,
      showCropper: false,
      imgSrc: "",
      cropperImgUrl: "",
      img: {},
      file: {},
      fileName: "",
      uploading: false,
      imgProgress: 0,
      fileProgress: 0,
      valid: false,
      formType: undefined,
      formTitle: "",
      nameRules: [
        v => !!v || this.$t("mod.inputTitle"),
        v => !v || v.length <= (this.$i18n.locale === "zh" ? 17 : 30) || this.$t("mod.lengthWarn")
      ],
      imgRules: [
        v => !!v || this.$t("mod.loadCoverPlz"),
        v => !v || v.size <= 5000000 || this.$t("mod.bigImgWarn"),
        v => !v || v.type.indexOf("image") > -1 || this.$t("mod.mustImg")
      ],
      fileRules: [
        v => !!v || this.$t("mod.uploadFilePlz"),
        v => !v || v.size <= 20000000 || this.$t("mod.bigFileWarn")
      ]
    };
  },
  computed: {
    items() {
      return [
        { text: this.$t("mod.character"), value: 0 },
        { text: this.$t("mod.trail"), value: 1 },
        { text: this.$t("mod.item"), value: 2 },
        { text: "HUD", value: 3 },
        { text: this.$t("mod.sound"), value: 4 },
        { text: this.$t("mod.bg"), value: 5 },
        { text: this.$t("mod.other"), value: 6 }
      ];
    },
    fileAccept() {
      let accept = "";
      switch (this.formType) {
        case 0:
        case 2:
          accept = ".png,.xnb";
          break;
        case 1:
          accept = ".png,.xnb,.srt";
          break;
        case 3:
        case 5:
          accept = ".png,.xnb,.zip,.rar";
          break;
        case 4:
          accept = ".xnb,.zip,.rar,.ogg";
          break;
        case 6:
          accept = ".png,.xnb,.srt,.zip,.rar,.ogg";
          break;
      }
      return accept;
    }
  },
  watch: {
    visible() {
      this.drawer = this.visible;
    }
  },
  methods: {
    changeImg(file) {
      if (file && file.type.indexOf("image") > -1) {
        this.cropperImgUrl = URL.createObjectURL(file);
        this.showCropper = true;
      }
    },
    changeFiles(file) {
      this.file = file;
    },
    getCroppedImg(blob) {
      this.imgSrc = URL.createObjectURL(blob);
      const metadata = {
        type: "image/jpeg"
      };
      const imgName = `${Date.now()}${Math.floor(Math.random() * 10)}`;
      this.img = new File([blob], imgName + ".jpg", metadata);
    },
    submitMod() {
      this.uploading = true;
      const that = this;
      getUploadToken().then(response => {
        const imgToken = response.data[0];
        const fileToken = response.data[1];
        that.uploadFile(imgToken, fileToken);
      });
    },
    uploadFile(imgToken, fileToken) {
      const that = this;
      // const putExtra = {
      //    fname: "qiniu.txt"
      //    customVars: { "x:test": "qiniu" },
      //    metadata: { "x-qn-meta": "qiniu" }
      // };
      const config = {
        useCdnDomain: true
      };
      // 上传封面
      const imgObservable = qiniu.upload(this.img, this.img.name, imgToken, null, config);
      const imgObserver = {
        next(res) {
          that.imgProgress = res.total.percent;
        },
        error(err) {
          that.$toast.error(`${err.name}:${err.message}`);
        },
        complete(res) {
          if (that.fileProgress === 100) {
            that.addModInfo();
          }
        }
      };
      imgObservable.subscribe(imgObserver);
      // 上传文件
      this.fileName = `${Date.now()}${Math.floor(Math.random() * 10)}.${this.file.name.split(".").pop()}`;
      const fileObservable = qiniu.upload(this.file, this.fileName, fileToken, null, config);
      const fileObserver = {
        next(res) {
          that.fileProgress = res.total.percent;
        },
        error(err) {
          that.$toast.error(`${err.name}:${err.message}`);
        },
        complete(res) {
          if (that.imgProgress === 100) {
            that.addModInfo();
          }
        }
      };
      fileObservable.subscribe(fileObserver);
    },
    addModInfo() {
      const that = this;
      const param = {};
      param.tag = this.formType;
      param.title = this.formTitle;
      param.imgUrl = this.img.name;
      param.fileUrl = this.fileName;
      param.size = this.file.size;
      addMod(param).then(res => {
        that.$toast.success(that.$t("mod.loadSucc"));
        that.$emit("success");
        that.uploading = false;
        that.doClose();
      }).catch(() => {
        that.uploading = false;
      });
    },
    clearImg() {
      this.$refs.imgInput.clearableCallback();
    },
    clearFiles() {
      this.$refs.fileInput.clearableCallback();
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