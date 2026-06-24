<template>
  <v-dialog
    v-model="visible"
    max-width="760"
    @click:outside="closeDialog()"
  >
    <v-card>
      <v-toolbar
        class="pl-4"
        color="primary"
      >
        <v-icon left>mdi-devices</v-icon>
        {{ $t('devices.title') }}
        <v-spacer />
        <v-btn icon @click="closeDialog">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </v-toolbar>

      <v-alert
        v-if="!loading && devices.length <= 1"
        class="ma-4 mb-0"
        dense
        text
        type="info"
      >
        {{ $t('devices.noOther') }}
      </v-alert>

      <v-data-table
        class="device-table"
        :headers="headers"
        :items="devices"
        :loading="loading"
        :no-data-text="$t('devices.noOther')"
        hide-default-footer
        disable-pagination
        @click:row="handleRowClick"
      >
        <template v-slot:item.deviceName="{ item }">
          <div class="device-name">
            <span>{{ item.deviceName }}</span>
            <v-chip
              v-if="item.isCurrent"
              class="ml-2"
              color="primary"
              x-small
            >
              {{ $t('devices.currentDevice') }}
            </v-chip>
          </div>
        </template>

        <template v-slot:item.loginDate="{ item }">
          {{ formatDate(item.loginDate) }}
        </template>

        <template v-slot:item.lastActiveTime="{ item }">
          {{ formatDate(item.lastActiveTime) }}
        </template>

        <template v-slot:item.actions="{ item }">
          <v-tooltip bottom>
            <template v-slot:activator="{ on, attrs }">
              <span v-bind="attrs" v-on="on">
                <v-btn
                  icon
                  :disabled="item.isCurrent || !item.canLogout"
                  @click.stop="logoutDevice(item)"
                >
                  <v-icon>mdi-logout-variant</v-icon>
                </v-btn>
              </span>
            </template>
            <span>{{ actionTip(item) }}</span>
          </v-tooltip>
        </template>
      </v-data-table>
    </v-card>
  </v-dialog>
</template>

<script>
import { getDevices, logoutOther } from "@/api/user";

export default {
  name: "DeviceManager",
  props: {
    visible: {
      type: Boolean,
      default: false
    }
  },
  data: () => ({
    loading: false,
    devices: []
  }),
  computed: {
    headers() {
      return [
        { text: this.$t("devices.deviceName"), value: "deviceName", sortable: false },
        { text: this.$t("devices.loginTime"), value: "loginDate" },
        { text: this.$t("devices.lastActive"), value: "lastActiveTime" },
        { text: "", value: "actions", sortable: false, align: "end", width: 64 }
      ];
    }
  },
  watch: {
    visible(value) {
      if (value) {
        this.loadDevices();
      }
    }
  },
  methods: {
    loadDevices() {
      this.loading = true;
      getDevices().then(response => {
        this.devices = response.data || [];
      }).finally(() => {
        this.loading = false;
      });
    },
    logoutDevice(item) {
      if (item.isCurrent || !item.canLogout) {
        this.showReloginHint(item);
        return;
      }
      logoutOther(item.tokenID).then(() => {
        this.$toast.success(this.$t("devices.logoutSucc"));
        this.loadDevices();
      }).catch(error => {
        if (error && error.code === -401) {
          this.$toast.info(this.$t("devices.reloginHint"));
        }
      });
    },
    handleRowClick(item) {
      if (!item.isCurrent && !item.canLogout) {
        this.showReloginHint(item);
      }
    },
    showReloginHint(item) {
      if (item.isCurrent) {
        return;
      }
      this.$toast.info(this.$t("devices.reloginHint"));
    },
    actionTip(item) {
      if (item.isCurrent) {
        return this.$t("devices.currentDevice");
      }
      return item.canLogout ? this.$t("devices.logoutThis") : this.$t("devices.cannotLogoutNewer");
    },
    formatDate(value) {
      if (!value) {
        return "-";
      }
      const date = new Date(value);
      if (Number.isNaN(date.getTime())) {
        return value;
      }
      return date.toLocaleString();
    },
    closeDialog() {
      this.$emit("update:visible", false);
    }
  }
};
</script>

<style lang="scss" scoped>
.device-table {
  width: 100%;
}
.device-name {
  align-items: center;
  display: flex;
  min-height: 36px;
}
</style>