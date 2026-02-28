import request from "@/utils/request";

// 获取个人主页数据
export function getProfileData(steamId) {
  return request({
    url: `/profile/getData/${steamId}`,
    method: "get"
  });
}

// 获取每日天梯分记录（用于热度图）
export function getDailyScoreHistory(steamId) {
  return request({
    url: `/profile/getDailyScoreHistory/${steamId}`,
    method: "get"
  });
}

// 获取个人成就
export function getAchievements(steamId) {
  return request({
    url: `/profile/getAchievements/${steamId}`,
    method: "get"
  });
}
