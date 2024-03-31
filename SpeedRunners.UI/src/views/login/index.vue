<template>
  <v-container fluid>
    <v-dialog
      value="true"
      persistent
      width="500"
    >
      <v-card>
        <v-card-title>
          {{ stepTitle }}
        </v-card-title>
        <v-progress-linear
          :indeterminate="step < 3"
        />
        <v-stepper v-model="step">
          <v-stepper-step
            :complete="step > 1"
            step="1"
          >
            {{ step1text }}
          </v-stepper-step>
          <v-stepper-step
            :complete="step > 2"
            step="2"
          >
            {{ step2text }}
          </v-stepper-step>
        </v-stepper>
      </v-card>
    </v-dialog>
  </v-container>
</template>
<script>
import { login } from "@/api/user";
import { initUserData } from "@/api/rank";
export default {
  name: "Login",
  props: { query: { type: String, default: null }},
  data: () => ({
    step: 1,
    timer: 3
  }),
  computed: {
    stepTitle() {
      return this.step > 2 ? this.$t("login.loginSuccess", [this.timer]) : this.$t("login.logging");
    },
    step1text() {
      return this.step > 1 ? this.$t("login.validateSucc") : this.$t("login.validating");
    },
    step2text() {
      let text = "";
      switch (this.step) {
        case 1:
          text = this.$t("login.initData");
          break;
        case 2:
          text = this.$t("login.initing");
          break;
        case 3:
          text = this.$t("login.initSucc");
          break;
      }
      return text;
    }
  },
  mounted() {
    this.handleLogin();
  },
  methods: {
    handleLogin() {
      login(this.query).then(response => {
        if (response.code === 666) {
          this.step = 2;
          initUserData().then(res => {
            if (res.code === 666) {
              this.loginSuccess();
            }
          });
        }
      });
    },
    loginSuccess() {
      this.step = 3;
      var that = this;
      var intervalId = setInterval(function() {
        that.timer--;
      }, 1000);

      var timeoutId = setTimeout(function() {
        clearInterval(intervalId);
        clearTimeout(timeoutId);
        that.$router.push("/");
      }, 3100);
    }
  }
};
</script>