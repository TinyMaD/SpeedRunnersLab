<template>
  <v-container fluid>
    <v-row justify="center" align="baseline">
      <div style="width:1500px">
        <v-row>
          <v-col cols="12" md="6" class="py-0 pr-md-1">
            <v-row>
              <v-col cols="12" class="py-0">
                <v-card class="mb-2 pa-2 rounded-0">
                  <div style="color:#03a245">{{ $t("index.online") }}</div>
                  <Odometer :value="onlineCount" class="text-h4" color="#03a245" />
                  <v-divider />
                  <v-tooltip v-for="player in playSRlist" :key="player.platformID" top>
                    <template v-slot:activator="{ on, attrs }">
                      <div class="d-inline-block ma-1" v-bind="attrs" v-on="on">
                        <UserAvatar
                          :platform-i-d="player.platformID"
                          :avatar-url="player.avatarM"
                          :size="48"
                        />
                      </div>
                    </template>
                    <span>{{ player.personaName }}</span>
                  </v-tooltip>
                  <AddedChart />
                  <p class="text-center mt-2">
                    <a
                      :href="$t('index.videoUrl')"
                      target="_blank"
                      class="text-decoration-underline"
                    >{{ $t('index.videoTitle') }}</a>
                  </p>
                </v-card>
              </v-col>
              <v-col cols="12" class="py-0">
                <v-card class="mb-2 pa-2 rounded-0">
                  <Sponsor />
                </v-card>
              </v-col>
            </v-row>
          </v-col>
          <v-col cols="12" md="6" class="py-0 pl-md-1">
            <v-row>
              <v-col class="py-0">
                <v-card class="mb-2 pa-2 rounded-0">
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
import Sponsor from "./sponsor";
import Odometer from "@/components/Odometer";
import UserAvatar from "@/components/UserAvatar";
import { getOnlineCount } from "@/api/steam";
import { getPlaySRList } from "@/api/rank";
export default {
  name: "Index",
  components: {
    AddedChart,
    HourChart,
    Odometer,
    Sponsor,
    UserAvatar
  },
  data: () => ({
    onlineCount: 0,
    playSRlist: []
  }),
  mounted() {
    getOnlineCount().then(response => {
      this.onlineCount = response.data;
    });
    getPlaySRList().then(response => {
      this.playSRlist = response.data;
    });
  }
};
</script>