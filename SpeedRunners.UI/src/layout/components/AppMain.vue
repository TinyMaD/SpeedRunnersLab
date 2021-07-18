<template>
  <section class="app-main">
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
  padding: 10px 0 10px 0;
}
.fixed-header + .app-main {
  padding-top: 50px;
}
</style>