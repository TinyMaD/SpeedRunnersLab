import request from "@/utils/request";

export function getUploadToken() {
  return request({
    url: "/asset/getUploadToken",
    method: "get"
  });
}
export function getDownloadUrl(fileName) {
  return request({
    url: `/asset/getDownloadUrl`,
    method: "post",
    data: { fileName }
  });
}
export function getModList(param) {
  return request({
    url: `/asset/getModList`,
    method: "post",
    data: param
  });
}
export function addMod(param) {
  return request({
    url: `/asset/addMod`,
    method: "post",
    data: param
  });
}
export function operateModStar(modID, star) {
  return request({
    url: `/asset/operateModStar/${modID}/${star}`,
    method: "get"
  });
}
export function getAfdianSponsor() {
  return request({
    url: `/asset/getAfdianSponsor`,
    method: "get"
  });
}