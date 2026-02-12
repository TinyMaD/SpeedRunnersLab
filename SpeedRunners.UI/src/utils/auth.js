import Cookies from "js-cookie";

const TokenKey = "srlab-token";
const Expires = 10 * 365;

export function getToken() {
  return Cookies.get(TokenKey);
}

export function setToken(token) {
  return Cookies.set(TokenKey, token, { expires: Expires });
}

export function removeToken() {
  return Cookies.remove(TokenKey);
}

export function goLoginURL() {
  const domain = window.location.host;
  const url = `https://steamcommunity.com/openid/login?openid.ns=http://specs.openid.net/auth/2.0&openid.mode=checkid_setup&openid.return_to=http://${domain}/login&openid.realm=http://${domain}&openid.identity=http://specs.openid.net/auth/2.0/identifier_select&openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select`;
  window.location = url;
}

// 判断用户是否被墙
export function isInChina() {
  return new Promise(resolve => {
    // 调用一个支持跨域请求的facebook接口
    const url = "https://graph.facebook.com/feed?callback=h";
    const xhr = new XMLHttpRequest();
    // 设置超时时间为2秒,提高稳定性
    xhr.timeout = 2000;

    xhr.open("GET", url);

    // 请求成功处理
    xhr.onload = () => {
      // 成功获取响应表示用户没有被墙
      resolve(false);
    };

    // 请求超时处理
    xhr.ontimeout = () => {
      // 请求超时表示用户在墙内
      resolve(true);
    };

    // 请求错误处理(网络错误、CORS错误等)
    xhr.onerror = () => {
      // 请求失败也认为用户在墙内
      resolve(true);
    };

    // 请求中止处理
    xhr.onabort = () => {
      // 请求中止也认为用户在墙内
      resolve(true);
    };

    xhr.send();
  });
}