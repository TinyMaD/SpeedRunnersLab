import Vue from "vue";
import "@mdi/font/css/materialdesignicons.css"; // Ensure you are using css-loader
import "normalize.css/normalize.css"; // A modern alternative to CSS resets
import "@/styles/index.scss"; // global css

import App from "./App";
import store from "./store";
import router from "./router";

import "@/icons"; // icon
import "@/permission"; // permission control
import vuetify from "./plugins/vuetify";
import "vuetify/dist/vuetify.min.css";

Vue.config.productionTip = false;
new Vue({
  el: "#app",
  router,
  store,
  vuetify,
  render: h => h(App)
});