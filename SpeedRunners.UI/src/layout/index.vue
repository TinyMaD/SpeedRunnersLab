<template>
  <div id="ap">
    <v-app-bar app hide-on-scroll fade-img-on-scroll dense>
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
      <!-- <v-tooltip bottom>
        <template v-slot:activator="{ on, attrs }">
          <v-btn
            depressed
            v-bind="attrs"
            @click="changeTheme()"
            v-on="on"
          ><v-icon>mdi-brightness-4</v-icon>
          </v-btn>
        </template>
        <span>浅色主题</span>
      </v-tooltip> -->
      <v-menu
        offset-y
        transition="slide-y-transition"
        open-on-hover
      >
        <template v-slot:activator="{ on, attrs }">
          <v-btn
            v-bind="attrs"
            depressed
            v-on="on"
          >
            <v-icon>mdi-translate</v-icon>
            <v-icon size="15">mdi-chevron-down</v-icon>
          </v-btn>

        </template>
        <v-list nav dense>
          <v-list-item-group @change="changeLanguege">
            <v-list-item>
              <v-list-item-content>
                <v-list-item-title>中文</v-list-item-title>
              </v-list-item-content>
            </v-list-item>
            <v-list-item>
              <v-list-item-content>
                <v-list-item-title>English</v-list-item-title>
              </v-list-item-content>
            </v-list-item>
          </v-list-item-group>
        </v-list>
      </v-menu>
      <template v-slot:extension>
        <v-tabs align-with-title>
          <v-tab v-for="route in navBars" :key="route.path" :to="route.path">
            <svg-icon :icon-class="route.meta.icon" />
            {{ $t(`routes.${route.meta.title}`) }}
          </v-tab>
        </v-tabs>
      </template>
    </v-app-bar>

    <v-navigation-drawer v-model="drawerRight" app clipped temporary>
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
            <v-icon left>mdi-steam</v-icon>{{ $t('layout.login') }}
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
              <v-list-item-title> {{ $t(`routes.${route.meta.title}`) }}</v-list-item-title>
            </v-list-item-content>
          </v-list-item>

          <v-list-item v-if="name && rankType != 0" @click="() => { privacyVisible = true }">
            <v-list-item-icon>
              <svg-icon icon-class="mdi-toggle-switch-outline" />
            </v-list-item-icon>
            <v-list-item-content>
              <v-list-item-title> {{ $t('routes.privacy') }}</v-list-item-title>
            </v-list-item-content>
          </v-list-item>

        </v-list-item-group>
      </v-list>
      <template v-if="avatar!==''" v-slot:append>
        <div class="pa-2">
          <v-btn block @click.stop="logout()">
            <v-icon left>mdi-exit-to-app</v-icon>{{ $t('layout.logout') }}
          </v-btn>
        </div>
      </template>
    </v-navigation-drawer>

    <PrivacySettings v-if="name && rankType != 0" :visible.sync="privacyVisible" />

    <v-main>
      <AppMain />
    </v-main>

    <v-footer class="body-2">
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
              v-bind="attrs"
              @click="copyEmail"
              v-on="on"
            >
              <svg-icon
                class-name="text-caption"
                icon-class="mdi-email"
              />
            </v-btn>
          </template>
          <span>{{ $t('layout.email') }}</span>
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
import PrivacySettings from "@/views/other/privacySettings";
import { goLoginURL } from "@/utils/auth";
import { mapGetters } from "vuex";
import getPageTitle from "@/utils/get-page-title";

export default {
  name: "Layout",
  components: {
    AppMain,
    PrivacySettings
  },
  data: () => ({
    drawer: null,
    drawerRight: false,
    right: false,
    left: false,
    fab: false,
    privacyVisible: false
  }),
  computed: {
    ...mapGetters([
      "name",
      "avatar",
      "rankType",
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
    changeTheme() {
      this.$vuetify.theme.dark = !this.$vuetify.theme.dark;
    },
    copyEmail() {
      navigator.clipboard.writeText("supremelang@qq.com");
      this.$toast.success(this.$t("layout.copyEmail"));
    },
    changeLanguege(num) {
      var lang = num ? "en" : "zh";
      this.$i18n.locale = lang;
      localStorage.setItem("lang", lang);
      document.title = getPageTitle(this.$i18n.t(`routes.${this.$route.meta.title}`));
    },
    logout() {
      var that = this;
      this.$store.dispatch("user/logoutLocal").then(() => {
        that.$router.push("/");
        that.$toast.info(that.$t("layout.logoutSucc"));
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