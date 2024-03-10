import request from "@/utils/request";

export function getInfo() {
  return request({
    url: "/user/getInfo",
    method: "get"
  });
}

export function login(query) {
  return request({
    url: "/user/login",
    method: "post",
    data: { query }
  });
}

export function logoutOther(tokenID) {
  return request({
    url: `/user/logoutOther/${tokenID}}`,
    method: "get"
  });
}

export function logoutLocal() {
  return request({
    url: "/user/logoutLocal",
    method: "get"
  });
}