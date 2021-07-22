<template>
  <v-dialog
    v-model="showCropper"
    persistent
    dark
    width="600"
  >
    <v-card min-width="600px" min-height="600px">
      <v-card-title>
        截取图片
      </v-card-title>
      <!-- <img ref="cropperImg" max-width="100%" max-height="100%" :src="cropperImgUrl"> -->
      <div style="width: 750px; height: 500px; margin: 20px; border: dashed #cacaca 1px; text-align: center;">
        <img ref="img" :src="cropperImgUrl" style="max-width: 100%">
      </div>
      <v-divider />
      <v-card-actions>
        <v-spacer />
        <v-btn
          color="primary"
          text
          @click="showCropper = false"
        >
          取 消
        </v-btn>
        <v-btn
          color="primary"
          @click="done"
        >
          确 定
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script>
import Cropper from "cropperjs";

export default {
  name: "ImgCropper",
  props: {
    visible: {
      type: Boolean,
      default: false
    },
    src: {
      type: String,
      default: ""
    }
  },
  data() {
    return {
      showCropper: this.visible,
      cropperImgUrl: "",
      cropper: "",
      imgName: ""
    };
  },
  watch: {
    visible() {
      this.$data.showCropper = this.visible;
    },
    showCropper() {
      if (this.showCropper === false) {
        this.$emit("update:visible", this.showCropper);
      }
    },
    src() {
      this.cropperImgUrl = this.src;
    }
  },
  mounted() {
    this.initCropper();
  },
  methods: {
    done() {
      this.initCropper();
    //  const canvas = this.cropper.getCroppedCanvas({
    //         width: 160,
    //         height: 160,
    //       });
    //       initialAvatarURL = avatar.src;
    //       avatar.src = canvas.toDataURL();
    },
    initCropper() {
      // console.log(this.$refs.cropperImg);
      // const cropper = new Cropper(this.$refs.cropperImg, {
      //   viewMode: 1,
      //   aspectRatio: 16 / 9
      // });
      // this.cropper = cropper;
      const cropper = new Cropper(this.$refs.img, {
        viewMode: 1,
        aspectRatio: 16 / 9
      });
      this.cropper = cropper;
    },
    destroyCropper() {
      this.cropper.destroy();
      this.cropper = null;
    }
  }
};
</script>