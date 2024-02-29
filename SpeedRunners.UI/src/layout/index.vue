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
            <svg-icon :icon-class="route.meta.icon" />
            {{ route.meta.title }}
          </v-tab>
        </v-tabs>
      </template>
    </v-app-bar>

    <v-navigation-drawer v-model="drawerRight" dark app clipped temporary>
      <v-list>
        <v-list-item v-if="avatar!==''">
          <v-list-item-avatar>
            <v-img :src="avatar" />
          </v-list-item-avatar>
          <v-list-item-content>
            <v-list-item-title>
              {{ name }}
            </v-list-item-title>
          </v-list-item-content>
        </v-list-item>
        <v-list-item v-else>
          <v-btn block light @click="goToSteamLogin()">
            <v-icon left>mdi-steam</v-icon>登 录
          </v-btn>
        </v-list-item>
      </v-list>
      <v-divider class="mt-2" />
      <v-list nav dense>
        <v-list-item-group>
          <v-list-item v-for="route in sideBars" :key="route.path" :to="route.path">
            <v-list-item-icon>
              <svg-icon :icon-class="route.meta.icon" />
            </v-list-item-icon>
            <v-list-item-content>
              <v-list-item-title>{{ route.meta.title }}</v-list-item-title>
            </v-list-item-content>
          </v-list-item>
        </v-list-item-group>
      </v-list>
      <template v-if="avatar!==''" v-slot:append>
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
        <v-tooltip top>
          <template v-slot:activator="{ on, attrs }">
            <v-btn
              class="mx-4"
              icon
              href="https://github.com/TinyMaD/SpeedRunnersLab"
              target="_blank"
              v-bind="attrs"
              v-on="on"
            >
              <svg-icon
                class-name="text-caption"
                icon-class="github2"
              />
            </v-btn>
          </template>
          <span>Github</span>
        </v-tooltip>

        <v-tooltip top>
          <template v-slot:activator="{ on, attrs }">
            <v-btn
              class="mx-4"
              icon
              href="https://space.bilibili.com/3857585"
              target="_blank"
              v-bind="attrs"
              v-on="on"
            >
              <svg-icon
                class-name="text-caption"
                icon-class="bilibili1"
              />
            </v-btn>
          </template>
          <span>Bilibili</span>
        </v-tooltip>

        <v-tooltip top>
          <template v-slot:activator="{ on, attrs }">
            <v-btn
              class="mx-4"
              icon
              href="https://steamcommunity.com/id/tinymad"
              target="_blank"
              v-bind="attrs"
              v-on="on"
            >
              <v-icon size="26px">mdi-steam</v-icon>
            </v-btn>
          </template>
          <span>Steam</span>
        </v-tooltip>

        <v-tooltip top>
          <template v-slot:activator="{ on, attrs }">
            <v-btn
              class="mx-4"
              icon
              href="https://www.cnblogs.com/tinymad"
              target="_blank"
              v-bind="attrs"
              v-on="on"
            >
              <svg-icon
                class-name="text-caption"
                icon-class="cnblogs1"
              />
            </v-btn>
          </template>
          <span>技术博客</span>
        </v-tooltip>
        <span class="caption">
          <br>Made with
          <v-icon color="red" size="14">mdi-heart</v-icon>&nbsp;for <a class="link" href="https://store.steampowered.com/app/207140/SpeedRunners" target="_blank">SpeedRunners</a><br>
        </span>
        <span class="caption">
          &copy;2018-{{ new Date().getFullYear() }} TinyMaD
          <br>
          <a style="color: white;" href="http://beian.miit.gov.cn" target="_blank">蜀ICP备18005857号-2</a><br>
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
    },
    sideBars() {
      return this.permission_routes.find(route => route.path === "/").children.filter(x => x.hidden === true && x.meta);
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