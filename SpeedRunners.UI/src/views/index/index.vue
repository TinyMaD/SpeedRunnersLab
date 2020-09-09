<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <v-row justify="center" align="baseline">
          <div style="width:1500px">
            <v-row>
              <v-col cols="12" md="6" class="py-0 pr-md-1">
                <v-row>
                  <v-col cols="12" class="py-0">
                    <v-card class="mb-2 pa-2" dark>
                      {{ onlineCount }}人在玩
                      <v-divider />
                      <v-avatar v-for="player in playSRlist" :key="player.platformID" class="ma-1">
                        <img
                          :src="player.avatarM"
                          :alt="player.personaName"
                          :title="player.personaName"
                        >
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
      </v-col>
    </v-row>
  </v-container>
</template>
<script>
import AddedChart from "./addedChart";
import HourChart from "./hourChart";
import { getOnlineCount } from "@/api/steam";
import { getPlaySRList } from "@/api/rank";
export default {
  name: "Index",
  components: {
    AddedChart,
    HourChart
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
  mounted: function() {
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