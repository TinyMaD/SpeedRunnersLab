import i18n from "@/i18n";

export const SITE_NAME = "SpeedRunnersLab";
export const SITE_ORIGIN = "https://www.speedrunners.cn";
export const DEFAULT_OG_IMAGE = `${SITE_ORIGIN}/img/og-default.png`;
export const BASE_KEYWORDS =
  "SpeedRunners,SpeedRunner,SR,SpeedRunnersLab,SRLab,SR国服,中国,国内,SR高手,SR高玩,SR数据统计,SR排行榜,SR记录,TinyMaD网站,TMD,数据可视化,撕逼,撕逼跑者,撕逼的Runners";

function normalizePath(path) {
  if (!path && typeof window !== "undefined") {
    return window.location.pathname || "/";
  }
  if (!path) return "/";
  return path.charAt(0) === "/" ? path : `/${path}`;
}

function toAbsoluteUrl(url) {
  if (!url) return DEFAULT_OG_IMAGE;
  if (/^https?:\/\//i.test(url)) return url;
  if (url.indexOf("//") === 0) return `https:${url}`;
  return `${SITE_ORIGIN}${url.charAt(0) === "/" ? url : `/${url}`}`;
}

export function buildMeta({ title, description, keywords, image, path, type = "website", jsonLd } = {}) {
  const fullTitle = title ? `${title} - ${SITE_NAME}` : SITE_NAME;
  const url = `${SITE_ORIGIN}${normalizePath(path)}`;
  const desc = description || i18n.t("seo.defaultDescription");
  const img = toAbsoluteUrl(image);
  const meta = [
    { vmid: "description", name: "description", content: desc },
    { vmid: "keywords", name: "keywords", content: keywords || BASE_KEYWORDS },
    { vmid: "og:title", property: "og:title", content: fullTitle },
    { vmid: "og:description", property: "og:description", content: desc },
    { vmid: "og:type", property: "og:type", content: type },
    { vmid: "og:url", property: "og:url", content: url },
    { vmid: "og:image", property: "og:image", content: img },
    { vmid: "og:site_name", property: "og:site_name", content: SITE_NAME },
    { vmid: "twitter:card", name: "twitter:card", content: "summary_large_image" },
    { vmid: "twitter:title", name: "twitter:title", content: fullTitle },
    { vmid: "twitter:description", name: "twitter:description", content: desc },
    { vmid: "twitter:image", name: "twitter:image", content: img }
  ];
  const out = {
    title: fullTitle,
    htmlAttrs: { lang: i18n.locale },
    meta,
    link: [{ vmid: "canonical", rel: "canonical", href: url }]
  };
  if (jsonLd) {
    out.script = [{ vmid: "ld-json", type: "application/ld+json", json: jsonLd }];
  }
  return out;
}

export function websiteJsonLd() {
  return {
    "@context": "https://schema.org",
    "@type": "WebSite",
    name: SITE_NAME,
    url: SITE_ORIGIN,
    potentialAction: {
      "@type": "SearchAction",
      target: `${SITE_ORIGIN}/searchplayer?q={q}`,
      "query-input": "required name=q"
    }
  };
}

export function personJsonLd(profile) {
  return {
    "@context": "https://schema.org",
    "@type": "ProfilePage",
    url: `${SITE_ORIGIN}/profile/${profile.platformID}`,
    mainEntity: {
      "@type": "Person",
      name: profile.personaName,
      image: toAbsoluteUrl(profile.avatarL || profile.avatarM),
      identifier: profile.platformID,
      sameAs: `https://steamcommunity.com/profiles/${profile.platformID}`
    }
  };
}

export function itemListJsonLd(players) {
  return {
    "@context": "https://schema.org",
    "@type": "ItemList",
    itemListElement: (players || []).slice(0, 100).map((player, index) => ({
      "@type": "ListItem",
      position: index + 1,
      name: player.personaName,
      url: `${SITE_ORIGIN}/profile/${player.platformID}`
    }))
  };
}

export function modJsonLd(mod) {
  return {
    "@context": "https://schema.org",
    "@type": "CreativeWork",
    name: mod.title,
    image: toAbsoluteUrl(mod.imgUrl),
    url: `${SITE_ORIGIN}/mod/${mod.id}`
  };
}