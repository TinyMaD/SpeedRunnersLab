import { getInfo, logoutLocal } from "@/api/user";
import { resetRouter } from "@/router";

const getDefaultState = () => {
  return {
    steamId: "",
    name: "",
    avatar: "",
    rankType: 0,
    participate: 0
  };
};

const state = getDefaultState();

const mutations = {
  RESET_STATE: (state) => {
    Object.assign(state, getDefaultState());
  },
  SET_STEAMID: (state, steamId) => {
    state.steamId = steamId;
  },
  SET_NAME: (state, name) => {
    state.name = name;
  },
  SET_AVATAR: (state, avatar) => {
    state.avatar = avatar;
  },
  SET_RANKTYPE: (state, rankType) => {
    state.rankType = rankType;
  },
  SET_PARTICIPATE: (state, participate) => {
    state.participate = participate;
  }
};

const actions = {
  // get user info
  getInfo({ commit }) {
    return new Promise((resolve, reject) => {
      getInfo().then(response => {
        const { data } = response;

        if (!data) {
          reject("获取信息失败，请重新登录");
        }

        const { personaName, avatarM, platformID, rankType, participate } = data;

        commit("SET_STEAMID", platformID);
        commit("SET_NAME", personaName);
        commit("SET_AVATAR", avatarM);
        commit("SET_RANKTYPE", rankType);
        commit("SET_PARTICIPATE", participate);
        resolve(data);
      }).catch(error => {
        reject(error);
      });
    });
  },

  // user logout
  logoutLocal({ commit }) {
    return new Promise((resolve, reject) => {
      logoutLocal().then(() => {
        resetRouter();
        commit("RESET_STATE");
        resolve();
      }).catch(error => {
        reject(error);
      });
    });
  },

  resetState({ commit }) {
    return new Promise(resolve => {
      commit("RESET_STATE");
      resolve();
    });
  }
};

export default {
  namespaced: true,
  state,
  mutations,
  actions
};