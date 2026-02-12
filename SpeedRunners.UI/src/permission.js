import router from "./router";
import store from "./store";
import i18n from "./i18n";
import Vue from "vue";
import NProgress from "nprogress"; // progress bar
import "nprogress/nprogress.css"; // progress bar style
import { getToken, isInChina } from "@/utils/auth"; // get token from cookie
import getPageTitle from "@/utils/get-page-title";
const version = require("@/utils/version");

NProgress.configure({ showSpinner: false }); // NProgress Configuration

// 版本检测状态标记
let isVersionChecking = false;

router.beforeEach(async(to, from, next) => {
  // start progress bar
  NProgress.start();

  // set page title
  document.title = getPageTitle(i18n.t(`routes.${to.meta.title}`));

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

  // 异步检测版本更新（不阻塞页面渲染）
  if (!isVersionChecking) {
    isVersionChecking = true;
    try {
      const versionInfo = await version.getPro();
      if (versionInfo.new) {
        console.log("[App] 检测到新版本，即将自动刷新...");
      }
    } catch (error) {
      // 静默处理错误，不影响用户体验
      console.log("[App] 版本检测失败，将在下次路由切换时重试");
    } finally {
      isVersionChecking = false;
    }
  }
});

// 页面可见性变化时检测版本（用户从其他标签页返回时）
document.addEventListener("visibilitychange", () => {
  if (document.visibilityState === "visible" && !isVersionChecking) {
    isVersionChecking = true;
    version.getPro()
      .then(versionInfo => {
        if (versionInfo.new) {
          console.log("[App] 页面重新可见，检测到新版本");
        }
      })
      .catch(() => {
        // 静默处理
      })
      .finally(() => {
        isVersionChecking = false;
      });
  }
});