using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.Comment;
using SpeedRunners.Utils;
using System;
using System.Linq;

namespace SpeedRunners.BLL
{
    public class CommentBLL : BLLHelper<CommentDAL>
    {
        private readonly NotificationBLL _notificationBLL;
        private readonly RankBLL _rankBLL;

        public CommentBLL(NotificationBLL notificationBLL, RankBLL rankBLL)
        {
            _notificationBLL = notificationBLL;
            _rankBLL = rankBLL;
        }

        /// <summary>
        /// 获取评论列表
        /// </summary>
        public MPageResult<MCommentOut> GetCommentList(MCommentPageParam param)
        {
            param.CurrentUserPlatformID = string.IsNullOrEmpty(CurrentUser?.PlatformID) ? null : CurrentUser.PlatformID;
            MPageResult<MCommentOut> result = new MPageResult<MCommentOut>();
            BeginDb(DAL =>
            {
                if (param.ParentID.HasValue)
                {
                    result = DAL.GetReplyList(param);
                }
                else
                {
                    result = DAL.GetCommentList(param);
                }
            });
            return result;
        }

        /// <summary>
        /// 添加评论
        /// </summary>
        public void AddComment(MAddComment param)
        {
            if (string.IsNullOrWhiteSpace(param.Content) || param.Content.Length > 2000)
            {
                throw new Exception(Localizer["comment_length_error"] ?? "评论内容不能为空且不能超过2000字");
            }

            int newCommentID = 0;
            BeginDb(DAL =>
            {
                MComment comment = new MComment
                {
                    PagePath = param.PagePath,
                    PlatformID = CurrentUser.PlatformID,
                    ParentID = param.ParentID,
                    ReplyToPlatformID = param.ReplyToPlatformID,
                    Content = param.Content.Trim(),
                    CreateTime = DateTime.Now
                };
                newCommentID = DAL.AddComment(comment);
            });

            // 发送回复通知
            if (!string.IsNullOrEmpty(param.ReplyToPlatformID))
            {
                SendReplyNotification(param.ReplyToPlatformID, param.PagePath, newCommentID, param.Content);
            }
            else if (param.ParentID.HasValue)
            {
                // 如果是回复顶级评论，通知顶级评论作者
                var parentComment = BeginDb(DAL => DAL.GetComment(param.ParentID.Value));
                if (parentComment != null && parentComment.PlatformID != CurrentUser.PlatformID)
                {
                    SendReplyNotification(parentComment.PlatformID, param.PagePath, newCommentID, param.Content);
                }
            }
        }

        private void SendReplyNotification(string receiverPlatformID, string pagePath, int commentID, string content)
        {
            if (receiverPlatformID == CurrentUser.PlatformID) return;

            // 获取发送者信息
            var senderInfo = _rankBLL.GetAllRankList(new[] { CurrentUser.PlatformID }).FirstOrDefault();
            string senderName = senderInfo?.PersonaName ?? CurrentUser.PlatformID;
            string senderAvatar = senderInfo?.AvatarS;

            _notificationBLL.AddReplyNotification(
                receiverPlatformID,
                CurrentUser.PlatformID,
                senderName,
                senderAvatar,
                commentID,
                "page",
                pagePath,
                content
            );
        }

        /// <summary>
        /// 删除评论
        /// </summary>
        public void DeleteComment(int commentID)
        {
            BeginDb(DAL =>
            {
                MComment comment = DAL.GetComment(commentID);
                if (comment == null)
                {
                    return;
                }

                // 只有评论作者或管理员可以删除
                if (comment.PlatformID != CurrentUser.PlatformID && CurrentUser.PlatformID != "76561198062688821")
                {
                    throw new Exception(Localizer["permission_error"] ?? "没有权限执行此操作");
                }

                DAL.DeleteComment(commentID);

                // 如果是顶级评论，同时删除所有回复
                if (!comment.ParentID.HasValue)
                {
                    DAL.DeleteReplies(commentID);
                }
            });
        }

        /// <summary>
        /// 切换点赞状态
        /// </summary>
        public int ToggleLike(int commentID)
        {
            int likeCount = 0;
            bool isLiked = false;
            MComment comment = null;

            BeginDb(DAL =>
            {
                // 先获取评论信息
                comment = DAL.GetComment(commentID);
                if (comment == null) return;

                // 执行点赞/取消点赞
                likeCount = DAL.ToggleLike(commentID, CurrentUser.PlatformID, out isLiked);
            });

            // 发送点赞通知（只有点赞时才发送，取消点赞不发送）
            if (isLiked && comment != null && comment.PlatformID != CurrentUser.PlatformID)
            {
                SendLikeNotification(comment, commentID);
            }

            return likeCount;
        }

        private void SendLikeNotification(MComment comment, int commentID)
        {
            // 获取发送者信息
            var senderInfo = _rankBLL.GetAllRankList(new[] { CurrentUser.PlatformID }).FirstOrDefault();
            string senderName = senderInfo?.PersonaName ?? CurrentUser.PlatformID;
            string senderAvatar = senderInfo?.AvatarS;

            _notificationBLL.AddLikeNotification(
                comment.PlatformID,
                CurrentUser.PlatformID,
                senderName,
                senderAvatar,
                commentID,
                "page",
                comment.PagePath,
                comment.Content
            );
        }
    }
}
