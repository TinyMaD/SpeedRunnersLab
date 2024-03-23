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
      v-html="content"
    />
    <a class="afd-btn" href="https://afdian.net/a/tinymad" target="_blank">
      <img width="150" style="border-radius:5px" src="https://pic1.afdiancdn.com/static/img/welcome/button-sponsorme.jpg">
    </a>
    <a class="paypal-btn" href="https://www.paypal.me/tinymad" target="_blank">
      <img width="150" style="border-radius:5px" src="https://www.paypalobjects.com/webstatic/mktg/logo/bdg_now_accepting_pp_2line_w.png">
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
      return this.$t("index.sponsorContent");
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

  .afd-btn{
    margin-left: 3px;
    margin-right: 4px;
    box-shadow: 0px 5px 10px rgba(#946ce6, 40%);
    :hover{
      box-shadow:0px 15px 25px -5px rgba(#946ce6, 40%);
      transform: scale(1.03);
      transition: all 0.3s;
    }
    :active{
      box-shadow:0px 4px 8px rgba(#946ce6, 30%);
      transform: scale(0.98);
      transition: all 0.3s;
    }
  }

  .paypal-btn{
    margin-left: 3px;
    box-shadow: 0px 5px 10px rgba(#1976d2, 40%);
    :hover{
      box-shadow:0px 15px 25px -5px rgba(#1976d2, 40%);
      transform: scale(1.03);
      transition: all 0.3s;
    }
    :active{
      box-shadow:0px 4px 8px rgba(#1976d2, 30%);
      transform: scale(0.98);
      transition: all 0.3s;
    }
  }
</style>