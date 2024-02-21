import axios from "axios";
import Vue from "vue";
import store from "@/store";
import { getToken, setToken, removeToken } from "@/utils/auth";

// create an axios instance
const service = axios.create({
  baseURL: process.env.VUE_APP_BASE_API, // url = base url + request url
  // withCredentials: true // send cookies when cross-domain requests
  timeout: 30000 // request timeout
});
// request interceptor
service.interceptors.request.use(
  config => {
    // do something before request is sent

    // let each request carry token
    const token = getToken();
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
    if (res.token === null) {
      removeToken();
    } else {
      setToken(res.token);
    }
    // if the custom code is not 666, it is judged as an error.
    if (res.code !== 666) {
      Vue.prototype.$toast.error(res.message || "Error");

      if (res.code === 50008 || res.code === 50012 || res.code === 50014) {
        // to re-login
        // MessageBox.confirm('You have been logged out, you can cancel to stay on this page, or log in again', 'Confirm logout', {
        //   confirmButtonText: 'Re-Login',
        //   cancelButtonText: 'Cancel',
        //   type: 'warning'
        // }).then(() => {
        //   store.dispatch('user/resetToken').then(() => {
        //     location.reload()
        //   })
        // })
        store.dispatch("user/resetToken").then(() => {
          location.reload();
        });
      }
      return Promise.reject(new Error(res.message || "Error"));
    } else {
      return res;
    }
  },
  error => {
    console.log("err" + error); // for debug
    Vue.prototype.$toast.error("恭喜您发现了彩蛋(BUG),站长表示正在改...");
    return Promise.reject(error);
  }
);

export default service;