<template>
  <div :class="className" :style="{width:width}">
    <div class="title text-h5 pa-2" v-text="title" />

    <v-tooltip v-for="sponsor in list" :key="sponsor.user.user_id" top>
      <template v-slot:activator="{ on, attrs }">
        <v-chip class="mt-3 mr-2" pill v-bind="attrs" v-on="on">
          <v-avatar left>
            <v-img :src="sponsor.user.avatar" />
          </v-avatar>
          ¥ {{ sponsor.all_sum_amount }}
        </v-chip>
      </template>
      <span>{{ sponsor.user.name }}</span>
    </v-tooltip>
    <div
      v-for="(content,i) in sponsorContent"
      :key="i"
      class="text-body-1 pa-1 my-1"
      v-text="content"
    />
    <a href="https://afdian.net/a/tinymad" target="_blank">
      <img width="200" style="border-radius:5px" src="https://pic1.afdiancdn.com/static/img/welcome/button-sponsorme.jpg">
    </a>
  </div>
</template>

<script>
import resize from "@/utils/resize";

import { getAfdianSponsor } from "@/api/asset";

export default {
  mixins: [resize],
  props: {
    className: {
      type: String,
      default: "chart"
    },
    width: {
      type: String,
      default: "100%"
    }
  },
  data() {
    return {
      totalCount: 0,
      list: []
    };
  },
  computed: {
    title() {
      return this.$t("index.sponsorTitle", [this.totalCount]);
    },
    sponsorContent() {
      return [
        this.$t("index.sponsorContent1"),
        this.$t("index.sponsorContent2"),
        this.$t("index.sponsorContent3")
      ];
    }
  },
  mounted() {
    var that = this;
    getAfdianSponsor().then(response => {
      that.totalCount = response.data.total_count;
      that.list = response.data.list;
    });
  }
};
</script>
<style lang="scss" scoped>
  .title {
    color: #e4c269;
    margin: 0 !important;
    border-bottom: 2px solid #e4c269;
    letter-spacing: 0 !important;
  }
</style>