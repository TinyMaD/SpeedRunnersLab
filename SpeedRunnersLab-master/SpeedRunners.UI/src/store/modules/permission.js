import { asyncRoutes, constantRoutes, add404Router } from "@/router";

const state = {
  routes: [],
  addRoutes: []
};

const mutations = {
  SET_ROUTES: (state, routes) => {
    state.addRoutes = routes;
    var routesMap = constantRoutes;
    var i = constantRoutes.findIndex(r => r.path === "/");
    var navBarRountes = routes.find(r => r.path === "/");
    if (navBarRountes) {
      routesMap[i].children = routesMap[i].children.concat(navBarRountes.children);
    }
    state.routes = routesMap;
  }
};

const actions = {
  generateRoutes({ commit }, isPlayer) {
    return new Promise(resolve => {
      let accessedRoutes = [];
      if (isPlayer) {
        // 加载异步路由
        accessedRoutes = asyncRoutes || [];
      }
      // 将404路由加载到最末位置
      accessedRoutes = accessedRoutes.concat(add404Router);
      commit("SET_ROUTES", accessedRoutes);
      resolve(accessedRoutes);
    });
  }
};

export default {
  namespaced: true,
  state,
  mutations,
  actions
};