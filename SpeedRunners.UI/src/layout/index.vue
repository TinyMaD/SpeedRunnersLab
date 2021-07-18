<template>
  <div id="ap">
    <v-app-bar app dark hide-on-scroll fade-img-on-scroll dense>
      <v-app-bar-nav-icon @click.stop="drawerRight = !drawerRight" />
      <v-toolbar-title>
        <v-img
          contain
          src="https://static.wixstatic.com/media/50a395_71b4e941fb774c2bbd0288a091774ff5~mv2.png/v1/fill/w_499,h_195,al_c/50a395_71b4e941fb774c2bbd0288a091774ff5~mv2.png"
          transition="scale-transition"
          width="170"
        />
      </v-toolbar-title>
      <VSpacer />
      <template v-slot:extension>
        <v-tabs align-with-title>
          <v-tab v-for="route in navBars" :key="route.path" :to="route.path">
            <v-icon class="ma-1">{{ route.meta.icon }}</v-icon>
            {{ route.meta.title }}
          </v-tab>
        </v-tabs>
      </template>
    </v-app-bar>

    <v-navigation-drawer v-model="drawerRight" dark app clipped temporary>
      <v-list>
        <v-list-item v-if="name!==''">
          <v-list-item-avatar>
            <v-img :src="avatar" />
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title v-text="name" />
          </v-list-item-content>
        </v-list-item>
        <v-list-item v-else>
          <v-btn block light @click="goToSteamLogin()">
            <v-icon left>mdi-steam</v-icon>登 录
          </v-btn>
        </v-list-item>
      </v-list>
      <template v-if="name!==''" v-slot:append>
        <div class="pa-2">
          <v-btn block @click.stop="logout()">
            <v-icon left>mdi-exit-to-app</v-icon>登 出
          </v-btn>
        </div>
      </template>
    </v-navigation-drawer>

    <v-main dark>
      <AppMain />
    </v-main>

    <v-footer dark class="body-2">
      <v-col class="text-center" cols="12">
        <span class="caption">
          &copy;2018-{{ new Date().getFullYear() }}
          <a class="link" href="https://space.bilibili.com/3857585" target="_blank">TinyMaD</a>
          <br>Made with
          <v-icon color="red" size="14">mdi-heart</v-icon>&nbsp;for SpeedRunners<br>
          <a class="link" href="http://beian.miit.gov.cn" target="_blank">蜀ICP备18005857号-2</a><br>
          <a class="link" href="https://github.com/TinyMaD/SpeedRunnersLab" target="_blank">网站源代码</a>
        </span>
      </v-col>
    </v-footer>

    <v-scale-transition origin="center center">
      <v-btn
        v-show="fab"
        v-scroll="onScroll"
        fab
        dark
        fixed
        bottom
        right
        color="primary"
        @click="toTop"
      >
        <v-icon>mdi-chevron-up</v-icon>
      </v-btn>
    </v-scale-transition>
  </div>
</template>

<script>
import { AppMain } from "./components";
import { goLoginURL } from "@/utils/auth";
import { mapGetters } from "vuex";

export default {
  name: "Layout",
  components: {
    AppMain
  },
  data: () => ({
    drawer: null,
    drawerRight: false,
    right: false,
    left: false,
    fab: false
  }),
  computed: {
    ...mapGetters([
      "name",
      "avatar",
      "permission_routes"
    ]),
    navBars() {
      return this.permission_routes.find(route => route.path === "/").children.filter(x => x.hidden !== true);
    }
  },
  methods: {
    logout() {
      var that = this;
      this.$store.dispatch("user/logoutLocal").then(() => {
        that.$router.push("/");
        that.$toast.info("登出成功");
      });
    },
    goToSteamLogin() {
      goLoginURL();
    },
    onScroll(e) {
      if (typeof window === "undefined") return;
      const top = window.pageYOffset || e.target.scrollTop || 0;
      this.fab = top > 20;
    },
    toTop() {
      this.$vuetify.goTo(0, {
        duration: 500,
        offset: 0,
        easing: "easeOutCubic"
      });
    }
  }
};
</script>

<style lang="scss" scoped>
@import "~@/styles/variables.scss";
#ap {
  background: url('../assets/bg.jpg') no-repeat center center fixed !important;
  background-size: cover;
}
.link {
  color: white;
  text-decoration: none;
}
.link:hover {
  color: white;
  text-decoration: underline;
}
</style>