<template>
  <section class="app-main" :style="{backgroundColor:backgroundColor}">
    <v-scroll-y-transition mode="out-in">
      <router-view :key="key" />
    </v-scroll-y-transition>
  </section>
</template>

<script>
export default {
  name: "AppMain",
  computed: {
    key() {
      return this.$route.path;
    },
    backgroundColor() {
      return this.$vuetify.theme.dark ? "rgba(0, 0, 0, 0.2)" : "rgba(255, 255, 255, 0.5)";
    }
  }
};
</script>

<style scoped>
.app-main {
  /*50 = navbar  */
  min-height: calc(100vh - 50px);
  width: 100%;
  position: relative;
  overflow: hidden;
  background:  url('../../assets/bg.jpg') rgba(255, 255, 255, 0.2) no-repeat center center fixed;
  background-blend-mode: overlay;
}
.fixed-header + .app-main {
  padding-top: 50px;
}
</style>