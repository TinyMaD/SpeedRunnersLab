<template>
  <section class="app-main" :style="{backgroundColor:backgroundColor}">
    <div class="content-wrapper" :class="{ 'with-comments': showComments }">
      <div class="main-content">
        <v-scroll-y-transition mode="out-in">
          <router-view :key="key" />
        </v-scroll-y-transition>
      </div>
      <v-scroll-y-transition mode="out-in">
        <CommentSection v-if="showComments" :key="'comments-' + key" class="comment-section-side mt-3" />
      </v-scroll-y-transition>
    </div>
  </section>
</template>

<script>
import CommentSection from "@/components/CommentSection";
import { getToken, isInChina } from "@/utils/auth";

export default {
  name: "AppMain",
  components: { CommentSection },
  data() {
    return {
      isInChinaResult: null
    };
  },
  computed: {
    key() {
      return this.$route.path;
    },
    backgroundColor() {
      return this.$vuetify.theme.dark ? "rgba(0, 0, 0, 0.2)" : "rgba(255, 255, 255, 0.5)";
    },
   showComments() {
      // 已登录用户始终显示评论区
      if (getToken()) return true;
      
      // 未登录用户：只有确认不在中国时才显示
      const isAbroad = this.isInChinaResult === false;
      return isAbroad;
    }
  },
  async created() {
    this.isInChinaResult = await isInChina();
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

.content-wrapper {
  display: flex;
  flex-direction: row;
  justify-content: center;
  align-items: flex-start;
  gap: 8px;
  padding: 16px;
  margin: 0 auto;
}

/* 主体内容区域 */
.main-content {
  flex: 0 1 auto;
  max-width: 100%;
}

/* 有评论区时的布局：主体内容偏左，紧贴评论区 */
.content-wrapper.with-comments {
  justify-content: flex-start;
  padding-left: max(16px, calc((100% - 1500px - 396px) / 2));
}

.content-wrapper.with-comments .main-content {
  flex: 0 0 auto;
}

.comment-section-side {
  flex: 0 0 380px;
  max-width: 380px;
  padding: 0 !important;
}

.comment-section-side >>> .v-container {
  max-width: 100%;
  margin: 0;
  padding: 0;
}

/* 空间充足时：主体内容完全居中（无评论区或屏幕很大） */
@media (min-width: 1920px) {
  .content-wrapper.with-comments {
    justify-content: center;
    padding-left: 16px;
  }
}

/* 中等屏幕：保持紧贴评论区布局 */
@media (max-width: 1600px) {
  .content-wrapper.with-comments {
    padding-left: 16px;
  }
}

/* 响应式布局：小屏幕时堆叠显示 */
@media (max-width: 1200px) {
  .content-wrapper,
  .content-wrapper.with-comments {
    flex-direction: column;
    align-items: center;
    padding-left: 16px;
  }

  .main-content {
    flex: 0 1 auto;
    width: 100%;
  }

  .comment-section-side {
    flex: 1 1 auto;
    max-width: 900px;
    width: 100%;
  }
}

.fixed-header + .app-main {
  padding-top: 50px;
}
</style>