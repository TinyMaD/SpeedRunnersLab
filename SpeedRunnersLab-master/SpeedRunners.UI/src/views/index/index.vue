<template>
  <v-container fluid>
    <v-row justify="center" align="baseline">
      <div style="width:1500px">
        <v-row>
          <v-col cols="12" md="6" class="py-0 pr-md-1">
            <v-row>
              <v-col cols="12" class="py-0">
                <v-card class="mb-2 pa-2" dark>
                  <div style="color:#81A636">全球在线</div>
                  <Odometer :value="onlineCount" class="text-h4" color="#81A636" />
                  <v-divider />
                  <v-avatar v-for="player in playSRlist" :key="player.platformID" class="ma-1">
                    <v-tooltip top>
                      <template v-slot:activator="{ on, attrs }">
                        <img
                          :src="player.avatarM"
                          v-bind="attrs"
                          v-on="on"
                        >
                      </template>
                      <span>{{ player.personaName }}</span>
                    </v-tooltip>
                  </v-avatar>
                </v-card>
              </v-col>
              <v-col cols="12" class="py-0">
                <v-card class="mb-2 pa-2" dark>
                  <AddedChart />
                </v-card>
              </v-col>
              <v-col cols="12" class="py-0">
                <v-card class="mb-2 pa-2" dark>
                  <div class="title text-h4 pa-2" v-text="'赞助'" />
                  <div
                    v-for="(content,i) in sponorContent"
                    :key="i"
                    class="text-body-1 pa-1 my-1"
                    v-text="content"
                  />
                  <a href="https://afdian.net/a/tinymad" target="_blank">
                    <img width="200" style="border-radius:5px" src="https://pic1.afdiancdn.com/static/img/welcome/button-sponsorme.jpg">
                  </a>
                </v-card>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="6" class="py-0 pl-md-1">
            <v-row>
              <v-col class="py-0">
                <v-card class="mb-2 pa-2" dark>
                  <HourChart />
                </v-card>
              </v-col>
            </v-row>
          </v-col>
        </v-row>
      </div>
    </v-row>
  </v-container>
</template>
<script>
import AddedChart from "./addedChart";
import HourChart from "./hourChart";
import Odometer from "@/components/Odometer";
import { getOnlineCount } from "@/api/steam";
import { getPlaySRList } from "@/api/rank";
export default {
  name: "Index",
  components: {
    AddedChart,
    HourChart,
    Odometer
  },
  data: () => ({
    onlineCount: 0,
    playSRlist: [],
    sponorContent: [
      "本网站的运行需要租用服务器、域名等，这也是最大的开销。服务器、域名到期后，网站则面临停运",
      "本站之前由我个人运营，但一个人的力量始终有限，一台最低配置的服务器也要七百左右一年",
      "如果本站给您带来了一点点帮助和感动的话，可以点击下方链接按钮赞助本站，让我继续为大家服务（所有的赞助记录，之后都会展示在本网站）"
    ]
  }),
  computed: {
    // cols() {
    //   const { xs, md, xl, lg, sm } = this.$vuetify.breakpoint;
    //   return lg ? "lg" : sm ? "sm" : xs ? "xs" : md ? "md" : xl ? "xl" : "";
    // }
  },
  mounted() {
    getOnlineCount().then(response => {
      this.onlineCount = response.data;
    });
    getPlaySRList().then(response => {
      this.playSRlist = response.data;
    });
  },
  methods: {
    // initChart() {}
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
  .sheett{
    padding-bottom: 1rem;
  }
</style>