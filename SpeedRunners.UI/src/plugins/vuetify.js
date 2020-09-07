import Vue from "vue";
import Vuetify, { VSnackbar, VBtn, VIcon } from "vuetify/lib";
import VuetifyToast from "vuetify-toast-snackbar-ng";

Vue.use(Vuetify, {
  components: {
    VSnackbar,
    VBtn,
    VIcon
  }
});
Vue.use(VuetifyToast, {
  x: "center",
  y: "top",
  showClose: true,
  timeout: 6000,
  closeIcon: "mdi-close"
});
export default new Vuetify({
  icons: {
    iconfont: "mdi"
  }
});