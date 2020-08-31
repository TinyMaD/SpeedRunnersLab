import request from "@/utils/request";

export function searchPlayer(keyWords) {
  return request({
    url: `/steam/searchPlayer/${keyWords}`,
    method: "get"
  });
}

export function getPlayerList(userName, sessionID, pageNo) {
  return request({
    url: `/steam/getPlayerList/${userName}/${sessionID}/${pageNo}`,
    method: "get"
  });
}

export function searchPlayerByUrl(url) {
  return request({
    url: `/steam/searchPlayerByUrl/${url}`,
    method: "get"
  });
}

export function searchPlayerBySteamID64(steamID64) {
  return request({
    url: `/steam/searchPlayerBySteamID64/${steamID64}`,
    method: "get"
  });
}