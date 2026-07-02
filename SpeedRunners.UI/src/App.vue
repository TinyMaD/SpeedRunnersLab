<template>
  <div id="app">
    <v-app>
      <div id="global" />
      <router-view />
    </v-app>
  </div>
</template>

<script>
import { buildMeta, websiteJsonLd } from "@/utils/seo";

export default {
  name: "App",
  metaInfo() {
    return buildMeta({
      description: this.$t("seo.defaultDescription"),
      jsonLd: websiteJsonLd()
    });
  }
};
</script>