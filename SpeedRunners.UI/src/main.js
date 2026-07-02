import Vue from "vue";
import "@mdi/font/css/materialdesignicons.css"; // Ensure you are using css-loader
import "normalize.css/normalize.css"; // A modern alternative to CSS resets
import "@/styles/index.scss"; // global css

import App from "./App";
import store from "./store";
import router from "./router";
import i18n from "./i18n";
import Meta from "vue-meta";
import { BASE_KEYWORDS } from "@/utils/seo";
import { dispatchPrerenderEvent } from "@/utils/prerender";

import "@/icons"; // icon
import "@/permission"; // permission control
import vuetify from "./plugins/vuetify";
import "vuetify/dist/vuetify.min.css";

Vue.config.productionTip = false;

Vue.use(Meta);
Vue.prototype.$baseKeywords = BASE_KEYWORDS;

if (window.__PRERENDER_INJECTED && window.__PRERENDER_INJECTED.prerender) {
  setTimeout(() => dispatchPrerenderEvent(), 15000);
}

new Vue({
  el: "#app",
  i18n,
  router,
  store,
  vuetify,
  render: h => h(App)
});