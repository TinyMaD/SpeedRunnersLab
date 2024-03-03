import defaultSettings from "@/settings";
import i18n from "@/i18n";

const title = defaultSettings.title || "Vue Admin Template";

export default function getPageTitle(pageTitle) {
  if (pageTitle) {
    return `${pageTitle.replace(/\s+/g, "")} - ${title}` + (i18n.locale === "zh" ? " - SR数据展示" : "");
  }
  return `${title}`;
}