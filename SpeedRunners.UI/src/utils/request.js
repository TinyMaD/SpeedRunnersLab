import axios from "axios";
import Vue from "vue";
import store from "@/store";
import i18n from "@/i18n";
import { getToken, setToken, removeToken } from "@/utils/auth";

// create an axios instance
const service = axios.create({
  baseURL: process.env.VUE_APP_BASE_API, // url = base url + request url
  // withCredentials: true // send cookies when cross-domain requests
  timeout: 30000 // request timeout
});

function isLoginRequest(config) {
  return config && config.url && config.url.toLowerCase() === "/user/login";
}

function canApplyTokenFromResponse(config) {
  return isLoginRequest(config) || (config.__srlabToken || "") === (getToken() || "");
}

// request interceptor
service.interceptors.request.use(
  config => {
    // do something before request is sent
    config.headers["locale"] = i18n.locale;
    // let each request carry token
    const token = getToken();
    config.__srlabToken = token || "";
    if (token) {
      config.headers["srlab-token"] = token;
    }
    return config;
  },
  error => {
    // do something with request error
    console.log(error); // for debug
    return Promise.reject(error);
  }
);

// response interceptor
service.interceptors.response.use(
  /**
   * If you want to get http information such as headers or status
   * Please return  response => response
  */

  /**
   * Determine the request status by custom code
   * Here is just an example
   * You can also judge the status by HTTP Status Code
   */
  response => {
    const res = response.data;
    const canApplyToken = canApplyTokenFromResponse(response.config);
    if (canApplyToken) {
      if (res.token === null) {
        removeToken();
      } else if (res.token) {
        setToken(res.token);
      }
    }
    // if the custom code is not 666, it is judged as an error.
    if (res.code !== 666) {
      const error = new Error(res.message || "Error");
      error.code = res.code;

      if (res.code === 50008 || res.code === 50012 || res.code === 50014) {
        if (canApplyToken) {
          Vue.prototype.$toast.error(res.message || i18n.t("devices.reloginHint"));
          store.dispatch("user/resetToken").then(() => {
            setTimeout(() => {
              window.location.href = "/";
            }, 800);
          });
        }
      } else {
        Vue.prototype.$toast.error(res.message || "Error");
      }
      return Promise.reject(error);
    } else {
      return res;
    }
  },
  error => {
    console.log("err" + error); // for debug
    Vue.prototype.$toast.error(i18n.t("500.msg"));
    return Promise.reject(error);
  }
);

export default service;