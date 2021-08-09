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
                  <Odometer :value="onlineCount" class="text-h4" style="color:#81A636" />
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
    playSRlist: []
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