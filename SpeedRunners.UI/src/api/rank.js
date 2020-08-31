import request from "@/utils/request";

export function getRankList() {
  return request({
    url: "/rank/getRankList",
    method: "get"
  });
}

export function asyncSRData() {
  return request({
    url: "/rank/asyncSRData",
    method: "get"
  });
}

export function initUserData() {
  return request({
    url: "/rank/initUserData",
    method: "get"
  });
}