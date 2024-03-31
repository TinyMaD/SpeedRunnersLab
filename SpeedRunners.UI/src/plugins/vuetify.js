import Vue from "vue";
import Vuetify, { VSnackbar, VBtn, VIcon } from "vuetify/lib";
import VuetifyToast from "vuetify-toast-snackbar-ng";
import zhHans from "vuetify/lib/locale/zh-Hans";

var themeDark = localStorage.getItem("themeDark") !== "false";
if (themeDark === null) {
  themeDark = true;
}
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
  lang: {
    locales: { zhHans },
    current: "zhHans"
  },
  icons: {
    iconfont: "mdi"
  },
  theme: { dark: themeDark }
});