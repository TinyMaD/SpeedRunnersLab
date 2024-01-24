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
    var url = "//graph.facebook.com/feed?callback=h";
    var xhr = new XMLHttpRequest();
    // 此接口响应迅速，1S足矣
    xhr.timeout = 1000;
    xhr.open("GET", url);
    xhr.onload = () => {
      if (xhr.readyState === 4 && xhr.status === 200) {
        // 获取到响应状态表示用户没有被墙
        return resolve(false);
      }
    };
    xhr.ontimeout = (e) => {
      // 请求超时表示用户在墙内
      return resolve(true);
    };
    xhr.send();
  });
}