<template>
  <div id="app">
    <v-app>
      <div id="global" />
      <router-view />
    </v-app>
  </div>
</template>

<script>
export default {
  name: "App",
  metaInfo() {
    return {
      meta: [
        {
          vmid: "description",
          name: "description",
          content:
            "欢迎来到SpeedRunnersLab(SRLab)。在这里可以浏览SpeedRunners高手们的游戏动态、数据资料、热门赛事等信息，同时提供丰富有趣的MOD资源。"
        },
        {
          vmid: "keywords",
          name: "keywords",
          content: this.$baseKeywords
        }
      ]
    };
  }
};
</script>