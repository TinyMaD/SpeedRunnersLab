<template>
  <v-container fluid>
    <v-dialog
      dark
      value="true"
      persistent
      width="500"
    >
      <v-card>
        <v-card-title>
          {{ stepTitle }}
        </v-card-title>
        <v-progress-linear
          :indeterminate="step<3"
        />
        <v-stepper v-model="step">
          <v-stepper-step
            :complete="step>1"
            step="1"
          >
            {{ step1text }}
          </v-stepper-step>
          <v-stepper-step
            :complete="step>2"
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
import { login, validateSteam } from "@/api/user";
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
      return this.step > 2 ? `登录成功，${this.timer} 秒后自动返回首页` : "正在登录，请耐心等待...";
    },
    step1text() {
      return this.step > 1 ? `验证登录信息成功` : "正在验证登录信息...";
    },
    step2text() {
      let text = "";
      switch (this.step) {
        case 1:
          text = "初始化用户数据";
          break;
        case 2:
          text = "正在初始化用户数据...";
          break;
        case 3:
          text = "初始化用户数据成功";
          break;
      }
      return text;
    }
  },
  mounted: function() {
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
        } else if (response.code === 555) {
          this.validate();
        }
      });
    },
    validate() {
      this.query.replace("(?<=openid.mode=).+?(?=\\&)", "check_authentication").Trim("?");
      validateSteam(this.query).then(response => {
        if (response.includes("is_valid:true")) {
          this.step = 2;
          initUserData().then(res => {
            if (res.code === 666) {
              this.loginSuccess();
            }
          });
        } else {
          this.$snackbar.error({ message: "登录失败" });
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