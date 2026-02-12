import request from "@/utils/request";

export function getCommentList(data) {
  return request({ url: "/comment/GetCommentList", method: "post", data });
}

export function addComment(data) {
  return request({ url: "/comment/AddComment", method: "post", data });
}

export function deleteComment(commentID) {
  return request({ url: `/comment/DeleteComment/${commentID}`, method: "get" });
}

export function toggleLike(commentID) {
  return request({ url: `/comment/ToggleLike/${commentID}`, method: "get" });
}
