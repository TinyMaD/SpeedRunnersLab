import Vue from "vue";
import Router from "vue-router";
Vue.use(Router);

/* Layout */
import Layout from "@/layout";

/**
 * Note: sub-menu only appear when route children.length >= 1
 * Detail see: https://panjiachen.github.io/vue-element-admin-site/guide/essentials/router-and-nav.html
 *
 * hidden: true                   if set true, item will not show in the sidebar(default is false)
 * alwaysShow: true               if set true, will always show the root menu
 *                                if not set alwaysShow, when item has more than one children route,
 *                                it will becomes nested mode, otherwise not show the root menu
 * redirect: noRedirect           if set noRedirect will no redirect in the breadcrumb
 * name:'router-name'             the name is used by <keep-alive> (must set!!!)
 * meta : {
    roles: ['admin','editor']    control the page roles (you can set multiple roles)
    title: 'title'               the name show in sidebar and breadcrumb (recommend set)
    icon: 'svg-name'             the icon show in the sidebar
    breadcrumb: false            if set false, the item will hidden in breadcrumb(default is true)
    activeMenu: '/example/list'  if set path, the sidebar will highlight the path you set
  }
 */

/**
 * constantRoutes
 * a base page that does not have permission requirements
 * all roles can be accessed
 */

export const constantRoutes = [
  {
    path: "/404",
    component: () => import("@/views/404")
  },
  {
    path: "/",
    component: Layout,
    children: [
      {
        path: "/",
        component: () => import("@/views/index/index"),
        meta: { title: "首 页", icon: "mdi-home-analytics" }
      },
      {
        path: "/match",
        component: () => import("@/views/match/index"),
        meta: { title: "赛 事", icon: "jiangbei" }
      },
      {
        path: "/rank",
        component: () => import("@/views/rank/index"),
        meta: { title: "排行榜", icon: "zhandouzuozhan" }
      },
      {
        path: "/mod",
        component: () => import("@/views/mod/index"),
        meta: { title: "MOD", icon: "mdi-file-download" }
      },
      {
        path: "/searchplayer",
        component: () => import("@/views/searchPlayer/index"),
        meta: { title: "资 料", icon: "mdi-account-search" }
      },
      {
        path: "/login",
        component: () => import("@/views/login/index"),
        props: route => ({ query: route.fullPath.split("?")[1] }),
        hidden: true
      },
      {
        path: "/log",
        component: () => import("@/views/other/log"),
        meta: { title: "更新日志", icon: "mdi-list-box" },
        hidden: true
      }
    ]
  }

  // {
  //   path: '/form',
  //   component: Layout,
  //   children: [
  //     {
  //       path: 'index',
  //       name: 'Form',
  //       component: () => import('@/views/form/index'),
  //       meta: { title: 'Form', icon: 'form' }
  //     }
  //   ]
  // },
];

// 异步挂载的路由
// 动态需要根据权限加载的路由表
export const asyncRoutes = [
  {
    path: "/",
    component: Layout,
    children: [
      {
        path: "/plaza",
        component: () => import("@/views/plaza/index"),
        meta: { title: "广 场", icon: "mdi-camera-iris" }
      }
    ]
  }
];

// 404路由
export const add404Router = [
  // 404 page must be placed at the end !!!
  { path: "*", redirect: "/404", hidden: true }
];

const createRouter = () =>
  new Router({
    mode: "history", // require service support
    scrollBehavior: () => ({ y: 0 }),
    routes: constantRoutes
  });

const router = createRouter();

// Detail see: https://github.com/vuejs/vue-router/issues/1234#issuecomment-357941465
export function resetRouter() {
  const newRouter = createRouter();
  router.matcher = newRouter.matcher; // reset router
}

export default router;