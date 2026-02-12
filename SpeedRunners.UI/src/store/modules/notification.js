import { getNotificationList, getUnreadCount, markAsRead } from "@/api/notification";

const state = {
  unreadCount: {
    replyCount: 0,
    likeCount: 0,
    totalCount: 0
  },
  notifications: [],
  total: 0,
  pollingTimer: null
};

const mutations = {
  SET_UNREAD_COUNT: (state, count) => {
    state.unreadCount = count;
  },
  SET_NOTIFICATIONS: (state, { list, total }) => {
    state.notifications = list;
    state.total = total;
  },
  ADD_NOTIFICATIONS: (state, { list }) => {
    state.notifications = [...state.notifications, ...list];
  },
  SET_POLLING_TIMER: (state, timer) => {
    state.pollingTimer = timer;
  },
  CLEAR_POLLING_TIMER: (state) => {
    if (state.pollingTimer) {
      clearInterval(state.pollingTimer);
      state.pollingTimer = null;
    }
  },
  MARK_AS_READ: (state, { type, ids }) => {
    // 更新本地消息状态
    state.notifications = state.notifications.map(item => {
      if ((type && item.type === type) || (ids && ids.includes(item.id))) {
        return { ...item, isRead: true };
      }
      return item;
    });

    // 更新未读数
    if (type === 1) {
      state.unreadCount.replyCount = 0;
    } else if (type === 2) {
      state.unreadCount.likeCount = 0;
    } else if (ids) {
      // 单个标记已读，重新计算
      const unreadReplies = state.notifications.filter(n => n.type === 1 && !n.isRead).length;
      const unreadLikes = state.notifications.filter(n => n.type === 2 && !n.isRead).length;
      state.unreadCount.replyCount = unreadReplies;
      state.unreadCount.likeCount = unreadLikes;
    }
    state.unreadCount.totalCount = state.unreadCount.replyCount + state.unreadCount.likeCount;
  }
};

const actions = {
  // 获取未读消息数量
  async fetchUnreadCount({ commit }) {
    try {
      const res = await getUnreadCount();
      if (res.code === 666) {
        commit("SET_UNREAD_COUNT", res.data);
      }
      return res;
    } catch (error) {
      console.error("获取未读消息数失败:", error);
      return null;
    }
  },

  // 获取消息列表
  async fetchNotifications({ commit }, params) {
    try {
      const res = await getNotificationList(params);
      if (res.code === 666) {
        if (params.pageIndex === 1) {
          commit("SET_NOTIFICATIONS", res.data);
        } else {
          commit("ADD_NOTIFICATIONS", res.data);
        }
      }
      return res;
    } catch (error) {
      console.error("获取消息列表失败:", error);
      return null;
    }
  },

  // 标记消息为已读
  async markAsRead({ commit, dispatch }, { type, notificationIDs }) {
    try {
      const res = await markAsRead({
        Type: type,
        NotificationIDs: notificationIDs
      });
      if (res.code === 666) {
        commit("MARK_AS_READ", { type, ids: notificationIDs });
        // 重新获取未读数
        dispatch("fetchUnreadCount");
      }
      return res;
    } catch (error) {
      console.error("标记已读失败:", error);
      return null;
    }
  },

  // 开始轮询
  startPolling({ commit, dispatch, state }) {
    // 先清除已有的定时器
    commit("CLEAR_POLLING_TIMER");

    // 立即获取一次
    dispatch("fetchUnreadCount");

    // 设置 30 秒轮询
    const timer = setInterval(() => {
      dispatch("fetchUnreadCount");
    }, 30000);

    commit("SET_POLLING_TIMER", timer);
  },

  // 停止轮询
  stopPolling({ commit }) {
    commit("CLEAR_POLLING_TIMER");
  }
};

export default {
  namespaced: true,
  state,
  mutations,
  actions
};