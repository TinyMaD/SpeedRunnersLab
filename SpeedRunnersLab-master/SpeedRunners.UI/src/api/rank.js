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

export function getPlaySRList() {
  return request({
    url: "/rank/getPlaySRList",
    method: "get"
  });
}

export function getAddedChart() {
  return request({
    url: "/rank/getAddedChart",
    method: "get"
  });
}

export function getHourChart() {
  return request({
    url: "/rank/getHourChart",
    method: "get"
  });
}

export function getSponsor() {
  return request({
    url: "/rank/getSponsor",
    method: "get"
  });
}

export function updateParticipate(participate) {
  return request({
    url: `/rank/updateParticipate/${participate}`,
    method: "get"
  });
}

export function getParticipateList() {
  return request({
    url: "/rank/getParticipateList",
    method: "get"
  });
}