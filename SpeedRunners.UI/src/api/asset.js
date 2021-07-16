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