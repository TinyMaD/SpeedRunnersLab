import router from "./router";
import store from "./store";
import Vue from "vue";
import NProgress from "nprogress"; // progress bar
import "nprogress/nprogress.css"; // progress bar style
import { getToken, isInChina } from "@/utils/auth"; // get token from cookie
import getPageTitle from "@/utils/get-page-title";
const version = require("@/utils/version");

NProgress.configure({ showSpinner: false }); // NProgress Configuration

router.beforeEach(async(to, from, next) => {
  // start progress bar
  NProgress.start();

  // set page title
  document.title = getPageTitle(to.meta.title);

  const hasToken = getToken();
  const hasRoles = store.getters.permission_routes && store.getters.permission_routes.length > 0;
  if (!hasRoles) {
    // 墙外或已登录用户需加载权限路由
    const isPlayer = hasToken || !await isInChina();
    // generate accessible routes map based on roles
    const accessRoutes = await store.dispatch("permission/generateRoutes", isPlayer);
    // dynamically add accessible routes
    router.addRoutes(accessRoutes);

    // hack method to ensure that addRoutes is complete
    // set the replace: true, so the navigation will not leave a history record
    next({ ...to, replace: true });
  } else {
    const hasGetUserInfo = store.getters.name;
    // determine whether the user has logged in
    if (!hasToken) {
      if (hasGetUserInfo) {
        await store.dispatch("user/resetState");
      }
      next();
    } else {
      if (hasGetUserInfo) {
        next();
      } else {
        try {
          // get user info
          await store.dispatch("user/getInfo").catch(() => {
            store.dispatch("user/resetState");
          });
          next();
        } catch (error) {
        // remove token and go to login page to re-login
          await store.dispatch("user/resetState");
          Vue.prototype.$toast.error(error || "Has Error");
          NProgress.done();
        }
      }
    }
  }
});

router.afterEach(async() => {
  // finish progress bar
  NProgress.done();
  // 如果不想每个路由都检查是否有新版本，只需要在特定的页面才需要检查版本，可以在这里做白名单判断
  // 兼容版本，如果是新版本则进行刷新并缓存
  const versionInfo = await version.getPro();
  console.log("versionInfo", versionInfo);
});