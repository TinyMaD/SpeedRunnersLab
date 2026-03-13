<template>
  <v-dialog
    v-model="visible"
    width="550"
    @click:outside="closeDialog()"
  >
    <v-card>
      <v-toolbar
        class="pl-4"
        color="primary"
      >{{ $t('routes.privacy') }}</v-toolbar>
      <v-list
        three-line
      >
        <!-- 个人主页开关 -->
        <v-list-item class="mx-4 mt-1">
          <v-list-item-content>
            <v-list-item-title>{{ $t('privacy.showProfile') }}</v-list-item-title>
            <v-list-item-subtitle>{{ $t('privacy.showProfileDetail') }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-switch
              v-model="settings.showProfile"
              color="primary"
              :false-value="0"
              :true-value="1"
              @change="changeShowProfile"
            />
          </v-list-item-action>
        </v-list-item>

        <v-divider />

        <v-list-item class="mx-4" :disabled="disabledShowProfile">
          <v-list-item-content>
            <v-list-item-title>{{ $t('privacy.publishState') }}</v-list-item-title>
            <v-list-item-subtitle>{{ $t('privacy.publishStateDetail') }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-switch
              v-model="settings.state"
              color="primary"
              :false-value="-1"
              :true-value="0"
              :disabled="disabledShowProfile"
              @change="changeState"
            />
          </v-list-item-action>
        </v-list-item>
        <v-list-item class="mx-4" :disabled="disabledShowProfile">
          <v-list-item-content>
            <v-list-item-title>{{ $t('privacy.publishPlaytime') }}</v-list-item-title>
            <v-list-item-subtitle>{{ $t('privacy.publishPlaytimeDetail') }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-switch
              v-model="settings.showWeekPlayTime"
              :false-value="0"
              :true-value="1"
              color="primary"
              :disabled="disabledShowProfile"
              @change="changeShowWeekPlayTime"
            />
          </v-list-item-action>
        </v-list-item>

        <v-divider />

        <v-list-item class="mx-4" :disabled="disabledShowProfile">
          <v-list-item-content>
            <v-list-item-title>{{ $t('privacy.allowGetRankScore') }}</v-list-item-title>
            <v-list-item-subtitle>{{ $t('privacy.allowGetRankScoreDetail') }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-switch
              v-model="settings.requestRankData"
              :false-value="0"
              :true-value="1"
              color="primary"
              :disabled="disabledShowProfile"
              @change="changeRequestRankData"
            />
          </v-list-item-action>
        </v-list-item>
        <v-list-item class="mr-4 ml-16" :disabled="disabledRequestRankData">
          <v-list-item-content>
            <v-list-item-title>{{ $t('privacy.publishAddScore') }}</v-list-item-title>
            <v-list-item-subtitle>{{ $t('privacy.publishAddScoreDetail') }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-switch
              v-model="settings.showAddScore"
              :false-value="0"
              :true-value="1"
              :disabled="disabledRequestRankData"
              color="primary"
              @change="changeShowAddScore"
            />
          </v-list-item-action>
        </v-list-item>
        <v-list-item class="mr-4 ml-16" :disabled="disabledRequestRankData">
          <v-list-item-content>
            <v-list-item-title>{{ $t('privacy.publishTotalScore') }}</v-list-item-title>
            <v-list-item-subtitle>{{ $t('privacy.publishTotalScoreDetail') }}</v-list-item-subtitle>
          </v-list-item-content>
          <v-list-item-action>
            <v-switch
              v-model="settings.rankType"
              :disabled="disabledRequestRankData"
              :false-value="2"
              :true-value="1"
              color="primary"
              @change="changeRankType"
            />
          </v-list-item-action>
        </v-list-item>
      </v-list>
    </v-card>
  </v-dialog>
</template>
<script>
import { getPrivacySettings, setRankType, setRequestRankData, setShowAddScore, setShowWeekPlayTime, setState, setShowProfile } from "@/api/user";
export default {
  props: {
    visible: {
      type: Boolean,
      default: false
    }
  },
  data: () => ({
    step: 1,
    settings: {
      state: -1,
      rankType: 2,
      requestRankData: 0,
      showAddScore: 0,
      showWeekPlayTime: 0,
      showProfile: 1
    }
  }),
  computed: {
    computedSettings() {
      return { ...this.settings };
    },
    disabledShowProfile() {
      return this.settings.showProfile === 0;
    },
    disabledRequestRankData() {
      return this.settings.showProfile === 0 || this.settings.requestRankData === 0;
    }
  },
  watch: {
    visible(value) {
      if (value) {
        this.getSettings();
      }
    }
  },
  methods: {
    changeShowProfile(value) {
      setShowProfile(value).then(response => {
        this.$toast.success(this.$t("common.success"));
      });
    },
    changeState(value) {
      setState(value).then(response => {
        this.$toast.success(this.$t("common.success"));
      });
    },
    changeRankType(value) {
      setRankType(value).then(response => {
        this.$toast.success(this.$t("common.success"));
      });
    },
    changeRequestRankData(value) {
      setRequestRankData(value).then(response => {
        this.$toast.success(this.$t("common.success"));
      }).finally(() => this.getSettings());
    },
    changeShowAddScore(value) {
      setShowAddScore(value).then(response => {
        this.$toast.success(this.$t("common.success"));
      });
    },
    changeShowWeekPlayTime(value) {
      setShowWeekPlayTime(value).then(response => {
        this.$toast.success(this.$t("common.success"));
      });
    },
    getSettings() {
      getPrivacySettings().then(response => {
        this.settings = response.data;
      });
    },
    closeDialog() {
      this.$emit("update:visible", false);
    }
  }
};
</script>
