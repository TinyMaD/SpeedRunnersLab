<template>
  <v-navigation-drawer
    v-model="drawerVisible"
    app
    right
    temporary
    width="400"
  >
    <v-card flat tile height="100%" class="d-flex flex-column">
      <!-- 标题栏 -->
      <v-card-title class="py-3 px-4">
        <v-icon left color="primary">mdi-bell-outline</v-icon>
        {{ $t('layout.notification') }}
        <v-spacer />
        <v-btn
          v-if="hasUnreadInCurrentTab"
          text
          small
          color="primary"
          @click="markAllAsRead"
        >
          {{ $t('common.markAllRead') || '全部已读' }}
        </v-btn>
      </v-card-title>

      <v-divider />

      <!-- 标签页 -->
      <v-tabs v-model="activeTab" grow class="flex-grow-0">
        <v-tab class="justify-center">
          <v-icon left small>mdi-message-reply-outline</v-icon>
          {{ $t('layout.replyMe') }}
          <v-badge
            v-if="unreadCount.replyCount > 0"
            :content="displayReplyCount"
            color="error"
            offset-x="-8"
            offset-y="8"
            class="ml-3"
          />
        </v-tab>
        <v-tab class="justify-center">
          <v-icon left small>mdi-heart-outline</v-icon>
          {{ $t('layout.likeMe') }}
          <v-badge
            v-if="unreadCount.likeCount > 0"
            :content="displayLikeCount"
            color="error"
            offset-x="-8"
            offset-y="8"
            class="ml-3"
          />
        </v-tab>
      </v-tabs>

      <v-divider />

      <!-- 消息列表 -->
      <v-tabs-items v-model="activeTab" class="flex-grow-1 overflow-hidden">
        <!-- 回复我 -->
        <v-tab-item class="fill-height">
          <v-list v-if="replyNotifications.length > 0" class="py-0 overflow-y-auto" style="max-height: calc(100vh - 180px);">
            <div
              v-for="(item, index) in replyNotifications"
              :key="item.id"
            >
              <v-list-item
                :class="getUnreadClass(item.isRead)"
                @click="handleNotificationClick(item)"
              >
                <v-list-item-avatar size="36">
                  <v-img :src="item.senderAvatar || '/img/default-avatar.png'" />
                </v-list-item-avatar>

                <v-list-item-content>
                  <v-list-item-title class="text-subtitle-2">
                    <span class="font-weight-bold">{{ item.senderName }}</span>
                    <span class="grey--text text--darken-1 ml-1">{{ $t('comment.reply') }}</span>
                  </v-list-item-title>
                  <v-list-item-subtitle class="text-caption mt-1 text-truncate">
                    {{ item.message }}
                  </v-list-item-subtitle>
                  <v-list-item-subtitle class="text-caption grey--text">
                    {{ formatTime(item.createTime) }}
                  </v-list-item-subtitle>
                </v-list-item-content>

                <v-list-item-action class="my-0">
                  <v-icon v-if="!item.isRead" x-small color="primary">mdi-circle</v-icon>
                </v-list-item-action>
              </v-list-item>

              <v-divider v-if="index < replyNotifications.length - 1" />
            </div>
          </v-list>

          <div
            v-else
            class="d-flex flex-column align-center justify-center mt-10 grey--text"
          >
            <v-icon size="64" color="grey lighten-2">mdi-message-text-outline</v-icon>
            <span class="mt-4">{{ $t('layout.noNotifications') }}</span>
          </div>
        </v-tab-item>

        <!-- 收到的点赞 -->
        <v-tab-item class="fill-height">
          <v-list v-if="likeNotifications.length > 0" class="py-0 overflow-y-auto" style="max-height: calc(100vh - 180px);">
            <div
              v-for="(item, index) in likeNotifications"
              :key="item.id"
            >
              <v-list-item
                :class="getUnreadClass(item.isRead)"
                @click="handleNotificationClick(item)"
              >
                <v-list-item-avatar size="36">
                  <v-img :src="item.senderAvatar || '/img/default-avatar.png'" />
                </v-list-item-avatar>

                <v-list-item-content>
                  <v-list-item-title class="text-subtitle-2">
                    <span class="font-weight-bold">{{ item.senderName }}</span>
                    <span class="pink--text ml-1">
                      <v-icon x-small color="pink">mdi-heart</v-icon>
                      {{ $t('comment.like') }}
                    </span>
                  </v-list-item-title>
                  <v-list-item-subtitle class="text-caption mt-1 text-truncate">
                    {{ item.message }}
                  </v-list-item-subtitle>
                  <v-list-item-subtitle class="text-caption grey--text">
                    {{ formatTime(item.createTime) }}
                  </v-list-item-subtitle>
                </v-list-item-content>

                <v-list-item-action class="my-0">
                  <v-icon v-if="!item.isRead" x-small color="primary">mdi-circle</v-icon>
                </v-list-item-action>
              </v-list-item>

              <v-divider v-if="index < likeNotifications.length - 1" />
            </div>
          </v-list>

          <div
            v-else
            class="d-flex flex-column align-center justify-center mt-10 grey--text"
          >
            <v-icon size="64" color="grey lighten-2">mdi-heart-outline</v-icon>
            <span class="mt-4">{{ $t('layout.noNotifications') }}</span>
          </div>
        </v-tab-item>
      </v-tabs-items>
    </v-card>
  </v-navigation-drawer>
</template>

<script>
import { mapState } from "vuex";

export default {
  name: "NotificationDrawer",
  props: {
    value: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      activeTab: 0,
      pageSize: 20
    };
  },
  computed: {
    ...mapState("notification", ["notifications", "unreadCount"]),
    drawerVisible: {
      get() {
        return this.value;
      },
      set(val) {
        this.$emit("input", val);
      }
    },
    replyNotifications() {
      return this.notifications.filter(n => n.type === 1);
    },
    likeNotifications() {
      return this.notifications.filter(n => n.type === 2);
    },
    displayReplyCount() {
      const count = this.unreadCount.replyCount || 0;
      return count > 99 ? '99+' : count;
    },
    displayLikeCount() {
      const count = this.unreadCount.likeCount || 0;
      return count > 99 ? '99+' : count;
    },
    // 当前选中的tab是否有未读消息
    hasUnreadInCurrentTab() {
      return this.activeTab === 0 
        ? (this.unreadCount.replyCount || 0) > 0 
        : (this.unreadCount.likeCount || 0) > 0;
    }
  },
  watch: {
    drawerVisible(val) {
      if (val) {
        this.loadNotifications();
      }
    },
    activeTab() {
      this.loadNotifications();
    }
  },
  methods: {
    getUnreadClass(isRead) {
      if (isRead) return '';
      return this.$vuetify.theme.dark ? 'unread-dark' : 'unread-light';
    },
    loadNotifications() {
      const type = this.activeTab === 0 ? 1 : 2;
      this.$store.dispatch("notification/fetchNotifications", {
        type,
        pageIndex: 1,
        pageSize: this.pageSize
      });
    },
    handleNotificationClick(item) {
      // 标记单条消息为已读
      if (!item.isRead) {
        this.$store.dispatch("notification/markAsRead", {
          notificationIDs: [item.id]
        });
      }

      // 跳转到对应页面
      if (item.contentType === "page" && item.contentTitle) {
        this.$router.push(item.contentTitle);
      }

      // 关闭抽屉
      this.drawerVisible = false;
    },
    markAllAsRead() {
      const type = this.activeTab === 0 ? 1 : 2;
      this.$store.dispatch("notification/markAsRead", { type });
    },
    formatTime(time) {
      if (!time) return "";
      const date = new Date(time);
      const now = new Date();
      const diff = now - date;

      // 小于1分钟
      if (diff < 60000) {
        return this.$t("comment.time.justNow");
      }
      // 小于1小时
      if (diff < 3600000) {
        return this.$t("comment.time.minutesAgo", [Math.floor(diff / 60000)]);
      }
      // 小于24小时
      if (diff < 86400000) {
        return this.$t("comment.time.hoursAgo", [Math.floor(diff / 3600000)]);
      }
      // 小于30天
      if (diff < 2592000000) {
        return this.$t("comment.time.daysAgo", [Math.floor(diff / 86400000)]);
      }
      return this.$t("comment.time.yearsAgo", [Math.floor(diff / 31536000000)]);
    }
  }
};
</script>

<style scoped>
.v-list-item {
  cursor: pointer;
  transition: background-color 0.2s;
}

.v-list-item:hover {
  background-color: rgba(0, 0, 0, 0.04) !important;
}
</style>

<style>
/* 未读消息高亮样式 - 暗色主题下用浅色底+深色字（反色） */
.unread-dark {
  background-color: #e0e0e0 !important;
}

.unread-dark .v-list-item__title,
.unread-dark .v-list-item__subtitle {
  color: rgba(0, 0, 0, 0.87) !important;
}

.unread-dark .v-list-item__title .font-weight-bold {
  color: rgba(0, 0, 0, 0.95) !important;
}

.unread-dark .grey--text {
  color: rgba(0, 0, 0, 0.6) !important;
}

/* 未读消息高亮样式 - 亮色主题下用深色底+浅色字（反色） */
.unread-light {
  background-color: #424242 !important;
}

.unread-light .v-list-item__title,
.unread-light .v-list-item__subtitle {
  color: rgba(255, 255, 255, 0.87) !important;
}

.unread-light .v-list-item__title .font-weight-bold {
  color: rgba(255, 255, 255, 0.95) !important;
}

.unread-light .grey--text {
  color: rgba(255, 255, 255, 0.6) !important;
}
</style>