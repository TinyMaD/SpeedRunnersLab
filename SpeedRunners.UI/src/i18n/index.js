import Vue from "vue";
import VueI18n from "vue-i18n";
import zh from "@/i18n/lang/zh.json";
import en from "@/i18n/lang/en.json";
import ru from "@/i18n/lang/ru.json";
import ptBr from "@/i18n/lang/pt-br.json";
import ja from "@/i18n/lang/ja.json";
import ko from "@/i18n/lang/ko.json";
import fr from "@/i18n/lang/fr.json";
import it from "@/i18n/lang/it.json";
import de from "@/i18n/lang/de.json";
import esEs from "@/i18n/lang/es-es.json";
import cs from "@/i18n/lang/cs.json";
import ro from "@/i18n/lang/ro.json";
import nl from "@/i18n/lang/nl.json";
import hu from "@/i18n/lang/hu.json";
import el from "@/i18n/lang/el.json";
import no from "@/i18n/lang/no.json";
import tr from "@/i18n/lang/tr.json";
import uk from "@/i18n/lang/uk.json";
import pl from "@/i18n/lang/pl.json";

Vue.use(VueI18n);

// 语言映射表：浏览器语言 -> 支持的语言代码
const langMap = {
  "zh": "zh",
  "zh-cn": "zh",
  "zh-tw": "zh",
  "zh-hk": "zh",
  "en": "en",
  "en-us": "en",
  "en-gb": "en",
  "ru": "ru",
  "pt": "pt-br",
  "pt-br": "pt-br",
  "ja": "ja",
  "ko": "ko",
  "ko-kr": "ko",
  "fr": "fr",
  "fr-fr": "fr",
  "it": "it",
  "it-it": "it",
  "de": "de",
  "de-de": "de",
  "de-at": "de",
  "es": "es-es",
  "es-es": "es-es",
  "cs": "cs",
  "cs-cz": "cs",
  "ro": "ro",
  "ro-ro": "ro",
  "nl": "nl",
  "nl-nl": "nl",
  "hu": "hu",
  "hu-hu": "hu",
  "el": "el",
  "el-gr": "el",
  "no": "no",
  "nb": "no",
  "nn": "no",
  "tr": "tr",
  "tr-tr": "tr",
  "uk": "uk",
  "uk-ua": "uk",
  "pl": "pl",
  "pl-pl": "pl"
};

const navLang = (navigator.language || navigator.userLanguage || navigator.browserLanguage).toLowerCase();
var localLang = navLang || false;
if (localStorage.getItem("lang") != null) {
  localLang = localStorage.getItem("lang");
} else {
  // get language from browser
  localLang = langMap[localLang] || "en";
  localStorage.setItem("lang", localLang);
}

// 实例化vue-i18n
const i18n = new VueI18n({

  // 从本地存储中取，如果没有默认为英文
  // 这样可以解决切换语言后，没记住选择的语言，刷新页面后还是默认的语言
  locale: localLang,

  messages: {
    "zh": zh,
    "en": en,
    "ru": ru,
    "pt-br": ptBr,
    "ja": ja,
    "ko": ko,
    "fr": fr,
    "it": it,
    "de": de,
    "es-es": esEs,
    "cs": cs,
    "ro": ro,
    "nl": nl,
    "hu": hu,
    "el": el,
    "no": no,
    "tr": tr,
    "uk": uk,
    "pl": pl
  }
});

export default i18n;