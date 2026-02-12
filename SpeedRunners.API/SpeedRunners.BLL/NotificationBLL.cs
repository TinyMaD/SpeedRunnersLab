using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;

namespace SpeedRunners.BLL
{
    public class NotificationBLL : BLLHelper<NotificationDAL>
    {

        /// <summary>
        /// 获取消息列表
        /// </summary>
        public MPageResult<MNotification> GetList(string receiverID, MNotificationQueryParam param)
        {
            return BeginDb(DAL => DAL.GetList(receiverID, param));
        }

        /// <summary>
        /// 获取未读消息数量
        /// </summary>
        public MUnreadCount GetUnreadCount(string receiverID)
        {
            return BeginDb(DAL => DAL.GetUnreadCount(receiverID));
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public void MarkAsRead(string receiverID, MMarkReadParam param)
        {
            BeginDb(DAL => DAL.MarkAsRead(receiverID, param));
        }

        /// <summary>
        /// 添加回复消息
        /// </summary>
        public void AddReplyNotification(string receiverID, string senderID, string senderName, string senderAvatar,
            int commentID, string contentType, string contentTitle, string message)
        {
            // 自己不给自己发消息
            if (receiverID == senderID) return;

            BeginDb(DAL =>
            {
                // 检查24小时内是否已存在相同消息
                if (DAL.Exists(receiverID, senderID, NotificationType.Reply, commentID)) return;

                var notification = new MNotification
                {
                    ReceiverID = receiverID,
                    SenderID = senderID,
                    SenderName = senderName,
                    SenderAvatar = senderAvatar,
                    Type = NotificationType.Reply,
                    ContentID = commentID,
                    ContentType = contentType,
                    ContentTitle = contentTitle?.Length > 50 ? contentTitle.Substring(0, 50) + "..." : contentTitle,
                    Message = message?.Length > 100 ? message.Substring(0, 100) + "..." : message
                };

                DAL.Add(notification);
            });
        }

        /// <summary>
        /// 添加点赞消息
        /// </summary>
        public void AddLikeNotification(string receiverID, string senderID, string senderName, string senderAvatar,
            int commentID, string contentType, string contentTitle, string message)
        {
            // 自己不给自己发消息
            if (receiverID == senderID) return;

            BeginDb(DAL =>
            {
                // 检查24小时内是否已存在相同消息
                if (DAL.Exists(receiverID, senderID, NotificationType.Like, commentID)) return;

                var notification = new MNotification
                {
                    ReceiverID = receiverID,
                    SenderID = senderID,
                    SenderName = senderName,
                    SenderAvatar = senderAvatar,
                    Type = NotificationType.Like,
                    ContentID = commentID,
                    ContentType = contentType,
                    ContentTitle = contentTitle?.Length > 50 ? contentTitle.Substring(0, 50) + "..." : contentTitle,
                    Message = message?.Length > 100 ? message.Substring(0, 100) + "..." : message
                };

                DAL.Add(notification);
            });
        }

        /// <summary>
        /// 清理过期消息
        /// </summary>
        public int CleanupExpired()
        {
            return BeginDb(DAL => DAL.DeleteExpired());
        }
    }
}
