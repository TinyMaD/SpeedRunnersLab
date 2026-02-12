using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.Comment;
using SpeedRunners.Utils;
using System;

namespace SpeedRunners.BLL
{
    public class CommentBLL : BLLHelper<CommentDAL>
    {
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
                DAL.AddComment(comment);
            });
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
            BeginDb(DAL =>
            {
                likeCount = DAL.ToggleLike(commentID, CurrentUser.PlatformID);
            });
            return likeCount;
        }
    }
}
