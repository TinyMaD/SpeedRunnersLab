import Vue from "vue";
import VueI18n from "vue-i18n";
import zh from "@/i18n/lang/zh.json";
import en from "@/i18n/lang/en.json";

Vue.use(VueI18n);

const navLang = (navigator.language || navigator.userLanguage || navigator.browserLanguage).toLowerCase();
var localLang = navLang || false;
if (localStorage.getItem("lang") != null) {
  localLang = localStorage.getItem("lang");
} else {
  // get language from browser
  if (localLang.toLowerCase().includes("zh") || localLang.toLowerCase().includes("cn")) {
    localLang = "zh";
  } else {
    localLang = "en";
  }
  localStorage.setItem("lang", localLang);
}

// 实例化vue-i18n
const i18n = new VueI18n({

  // 从本地存储中取，如果没有默认为中文，
  // 这样可以解决切换语言后，没记住选择的语言，刷新页面后还是默认的语言
  locale: localLang,

  messages: {
    "zh": zh, // 中文语言包
    "en": en // 英文语言包
  }
});

export default i18n;