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

export function getPrivacySettings() {
  return request({
    url: "/user/GetPrivacySettings",
    method: "get"
  });
}

export function setState(value) {
  return request({
    url: "/user/SetState",
    method: "post",
    data: { value }
  });
}

export function setRankType(value) {
  return request({
    url: "/user/setRankType",
    method: "post",
    data: { value }
  });
}

export function setShowWeekPlayTime(value) {
  return request({
    url: "/user/setShowWeekPlayTime",
    method: "post",
    data: { value }
  });
}

export function setRequestRankData(value) {
  return request({
    url: "/user/setRequestRankData",
    method: "post",
    data: { value }
  });
}

export function setShowAddScore(value) {
  return request({
    url: "/user/setShowAddScore",
    method: "post",
    data: { value }
  });
}