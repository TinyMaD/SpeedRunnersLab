using System;

namespace SpeedRunners.Model.User
{
    /// <summary>
    /// 消息通知类型
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// 回复我
        /// </summary>
        Reply = 1,

        /// <summary>
        /// 收到的点赞
        /// </summary>
        Like = 2
    }

    /// <summary>
    /// 消息通知实体
    /// </summary>
    public class MNotification
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// 接收用户PlatformID
        /// </summary>
        public string ReceiverID { get; set; }

        /// <summary>
        /// 发送用户PlatformID
        /// </summary>
        public string SenderID { get; set; }

        /// <summary>
        /// 发送用户名称
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// 发送用户头像
        /// </summary>
        public string SenderAvatar { get; set; }

        /// <summary>
        /// 消息类型：1-回复我 2-收到的点赞
        /// </summary>
        public NotificationType Type { get; set; }

        /// <summary>
        /// 关联内容ID（如评论ID）
        /// </summary>
        public int ContentID { get; set; }

        /// <summary>
        /// 关联内容类型（如mod、page等）
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// 关联内容标题/摘要
        /// </summary>
        public string ContentTitle { get; set; }

        /// <summary>
        /// 消息内容摘要
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>
        public DateTime? ReadTime { get; set; }
    }

    /// <summary>
    /// 消息查询参数
    /// </summary>
    public class MNotificationQueryParam : MPageParam
    {
        /// <summary>
        /// 消息类型：null-全部 1-回复我 2-收到的点赞
        /// </summary>
        public NotificationType? Type { get; set; }

        /// <summary>
        /// 是否已读：null-全部 true-已读 false-未读
        /// </summary>
        public bool? IsRead { get; set; }
    }

    /// <summary>
    /// 未读消息数量统计
    /// </summary>
    public class MUnreadCount
    {
        /// <summary>
        /// 回复我的未读数
        /// </summary>
        public int ReplyCount { get; set; }

        /// <summary>
        /// 收到点赞的未读数
        /// </summary>
        public int LikeCount { get; set; }

        /// <summary>
        /// 总未读数
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// 标记已读参数
    /// </summary>
    public class MMarkReadParam
    {
        /// <summary>
        /// 消息ID列表，为空则标记该类型所有消息为已读
        /// </summary>
        public int[] NotificationIDs { get; set; }

        /// <summary>
        /// 消息类型：null-所有类型
        /// </summary>
        public NotificationType? Type { get; set; }
    }
}
