import request from "@/utils/request";

export function getNotificationList(data) {
  return request({ url: "/notification/GetList", method: "post", data });
}

export function getUnreadCount() {
  return request({ url: "/notification/GetUnreadCount", method: "get" });
}

export function markAsRead(data) {
  return request({ url: "/notification/MarkAsRead", method: "post", data });
}