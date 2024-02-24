<template>
  <div :class="className" :style="{width:width}">
    <div class="title text-h5 pa-2" v-text="title" />
    
     <v-tooltip top v-for="sponsor in list" :key="sponsor.user.user_id">
      <template v-slot:activator="{ on, attrs }">
        <v-chip class="mt-3 mr-2" pill v-bind="attrs" v-on="on">
        <v-avatar left>
        <v-img :src="sponsor.user.avatar"></v-img>
        </v-avatar>
        ¥ {{sponsor.all_sum_amount}}
      </v-chip>
      </template>
      <span>{{sponsor.user.name}}</span>
    </v-tooltip>
    <div
      v-for="(content,i) in sponorContent"
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
      list: [],
      sponorContent: [
        "本网站的运行需要租用服务器、域名等，这也是最大的开销。服务器、域名到期后，网站则面临停运",
        "本站之前由我个人运营，但一个人的力量始终有限，一台最低配置的服务器也要七百左右一年",
        "如果本站给您带来了一点点帮助和感动的话，可以点击下方链接按钮赞助本站，让我继续为大家服务（爱发电的赞助记录会实时展示在上方，收支明细在爱发电动态）"
      ]
    };
  },
  mounted() {
    var that = this;
    getAfdianSponsor().then(response => {
      that.totalCount = response.data.total_count;
      that.list = response.data.list;
    });
  },
  computed: {
    title() {
      return `感谢 ${this.totalCount} 名赞助者`;
    }
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