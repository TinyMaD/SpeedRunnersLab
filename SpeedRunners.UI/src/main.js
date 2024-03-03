import Vue from "vue";
import "@mdi/font/css/materialdesignicons.css"; // Ensure you are using css-loader
import "normalize.css/normalize.css"; // A modern alternative to CSS resets
import "@/styles/index.scss"; // global css

import App from "./App";
import store from "./store";
import router from "./router";
import i18n from "./i18n";
import Meta from "vue-meta";

import "@/icons"; // icon
import "@/permission"; // permission control
import vuetify from "./plugins/vuetify";
import "vuetify/dist/vuetify.min.css";

Vue.config.productionTip = false;

Vue.use(Meta);
Vue.prototype.$baseKeywords =
  "SpeedRunners,SpeedRunner,SR,SpeedRunnersLab,SRLab,SR国服,中国,国内,SR高手,SR高玩,SR数据统计,SR排行榜,SR记录,TinyMaD网站,TMD,数据可视化,撕逼,撕逼跑者,撕逼的Runners";

new Vue({
  el: "#app",
  i18n,
  router,
  store,
  vuetify,
  render: h => h(App)
});