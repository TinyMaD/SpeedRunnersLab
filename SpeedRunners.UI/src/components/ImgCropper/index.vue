<template>
  <v-layout row align-content-space-around xs12>
    <v-dialog
      v-model="dialog"
      :max-width="dialogMaxWidth"
      :max-height="dialogMaxHeight"
      hide-overlay
      persistent
      :disabled="!imgSrc"
    >
      <v-card>
        <v-card-title> <span :class="isZh ? 'headline' : ''">{{ $t('components.cropper.title') }}</span> </v-card-title>
        <v-card-text>
          <vue-cropper
            ref="cropper"
            :aspect-ratio="250 / 160"
            :view-mode="2"
            drag-mode="move"
            :auto-crop-area="1"
            :min-container-width="250"
            :min-container-height="160"
            :background="true"
            :rotatable="true"
            :src="imgSrc"
            :modal="true"
            :img-style="{ width: '500px', height: '500px' }"
            :center="false"
            :highlight="true"
          />
        </v-card-text>
        <v-card-actions>
          <v-tooltip top>
            <template v-slot:activator="{ on, attrs }">
              <v-icon
                color="blue"
                v-bind="attrs"
                v-on="on"
                @click="cropImage"
              >
                mdi-crop
              </v-icon>
            </template>
            <span>{{ $t('components.cropper.crop') }}</span>
          </v-tooltip>
          <v-icon color="blue" @click="rotate('r');">mdi-rotate-right</v-icon>
          <v-icon color="blue" @click="rotate('l');">mdi-rotate-left</v-icon>
          <v-icon color="blue" @click="flip(true);">mdi-flip-vertical</v-icon>
          <v-icon color="blue" @click="flip(false);">mdi-flip-horizontal</v-icon>

          <v-spacer />

          <v-btn
            color="blue"
            text
            @click="dialog = false"
          >{{ $t('common.cancel') }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-layout>
</template>

<script>
import VueCropper from "vue-cropperjs";
import "cropperjs/dist/cropper.css";

export default {
  components: {
    VueCropper
  },
  props: {
    dialogMaxWidth: { type: String, default: "600px" },
    dialogMaxHeight: { type: String, default: "0.8vh" },
    maxWidth: { type: Number, default: 1920 },
    maxHeight: { type: Number, default: 1200 },
    // the URL of the blob image
    src: {
      type: String,
      default: ""
    },
    visible: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      imgSrc: "",
      dialog: false,
      file: null,
      filename: null,
      cropBlob: null
    };
  },
  computed: {
    isZh() {
      return this.$i18n.locale === "zh";
    }
  },
  watch: {
    src() {
      this.imgSrc = this.src;
      this.$refs.cropper && this.$refs.cropper.replace(this.src);
    },
    visible() {
      this.$data.dialog = this.visible;
    },
    dialog() {
      this.$emit("update:visible", this.dialog);
    }
  },
  methods: {
    cropImage() {
      // get image data for post processing, e.g. upload or setting image src
      // this.cropImg = this.$refs.cropper.getCroppedCanvas().toDataURL()
      this.$refs.cropper
        .getCroppedCanvas({
          maxWidth: this.maxWidth,
          maxHeight: this.maxHeight
        })
        .toBlob(
          blob => {
            this.$emit("done", blob);
          },
          "image/jpeg",
          0.95
        );
      this.dialog = false;
    },
    rotate(dir) {
      if (dir === "r") {
        this.$refs.cropper.rotate(90);
      } else {
        this.$refs.cropper.rotate(-90);
      }
    },
    flip(vert) {
      const { scaleX, scaleY, rotate } = this.$refs.cropper.getData();
      // when image is rotated, direction must be flipped
      if (rotate === 90 || rotate === 270) {
        vert = !vert;
      }
      if (vert) {
        this.$refs.cropper.scale(scaleX, -1 * scaleY);
      } else {
        this.$refs.cropper.scale(-1 * scaleX, scaleY);
      }
    }
  }
};
</script>

<style lang="scss" scoped>
.v-icon.v-icon.v-icon--link {
  padding: 0 10px;
}
</style>