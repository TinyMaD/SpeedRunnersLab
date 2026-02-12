using SpeedRunners.Model;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunners.DAL
{
    public class NotificationDAL : DALBase
    {
        public NotificationDAL(DbHelper db) : base(db) { }

        /// <summary>
        /// 获取消息列表
        /// </summary>
        public MPageResult<MNotification> GetList(string receiverID, MNotificationQueryParam param)
        {
            string whereSql = $"WHERE ReceiverID = ?{nameof(receiverID)}";
            var parameters = new Dictionary<string, object> { { nameof(receiverID), receiverID } };

            if (param.Type.HasValue)
            {
                whereSql += $" AND Type = ?type";
                parameters.Add("type", (int)param.Type.Value);
            }

            if (param.IsRead.HasValue)
            {
                whereSql += $" AND IsRead = ?isRead";
                parameters.Add("isRead", param.IsRead.Value ? 1 : 0);
            }

            // 只显示30天内的消息
            whereSql += " AND CreateTime >= DATE_SUB(NOW(), INTERVAL 30 DAY)";

            string countSql = $@"SELECT COUNT(*) FROM Notification {whereSql}";
            int total = Db.ExecuteScalar<int>(countSql, parameters);

            string dataSql = $@"
                SELECT 
                    ID,
                    ReceiverID,
                    SenderID,
                    SenderName,
                    SenderAvatar,
                    Type,
                    ContentID,
                    ContentType,
                    ContentTitle,
                    Message,
                    IsRead,
                    CreateTime,
                    ReadTime
                FROM Notification
                {whereSql}
                ORDER BY CreateTime DESC
                LIMIT ?pageSize OFFSET ?offset";

            parameters.Add("pageSize", param.PageSize);
            parameters.Add("offset", param.Offset);

            var list = Db.Query<MNotification>(dataSql, parameters).ToList();

            return new MPageResult<MNotification>
            {
                List = list,
                Total = total
            };
        }

        /// <summary>
        /// 获取未读消息数量
        /// </summary>
        public MUnreadCount GetUnreadCount(string receiverID)
        {
            string sql = $@"
                SELECT 
                    SUM(CASE WHEN Type = 1 AND IsRead = 0 THEN 1 ELSE 0 END) as ReplyCount,
                    SUM(CASE WHEN Type = 2 AND IsRead = 0 THEN 1 ELSE 0 END) as LikeCount,
                    SUM(CASE WHEN IsRead = 0 THEN 1 ELSE 0 END) as TotalCount
                FROM Notification
                WHERE ReceiverID = ?{nameof(receiverID)}
                AND CreateTime >= DATE_SUB(NOW(), INTERVAL 30 DAY)";

            return Db.QueryFirstOrDefault<MUnreadCount>(sql, new { receiverID })
                ?? new MUnreadCount { ReplyCount = 0, LikeCount = 0, TotalCount = 0 };
        }

        /// <summary>
        /// 添加消息
        /// </summary>
        public void Add(MNotification notification)
        {
            notification.CreateTime = DateTime.Now;
            notification.IsRead = false;
            Db.Insert("Notification", notification, new[] { nameof(notification.ID), nameof(notification.ReadTime) });
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        public int MarkAsRead(string receiverID, MMarkReadParam param)
        {
            string whereSql = $"WHERE ReceiverID = ?{nameof(receiverID)} AND IsRead = 0";
            var parameters = new Dictionary<string, object> { { nameof(receiverID), receiverID } };

            if (param.NotificationIDs?.Length > 0)
            {
                whereSql += $" AND ID IN ({string.Join(",", param.NotificationIDs)})";
            }

            if (param.Type.HasValue)
            {
                whereSql += $" AND Type = ?type";
                parameters.Add("type", (int)param.Type.Value);
            }

            string sql = $@"
                UPDATE Notification 
                SET IsRead = 1, ReadTime = ?readTime
                {whereSql}";

            parameters.Add("readTime", DateTime.Now);

            return Db.Execute(sql, parameters);
        }

        /// <summary>
        /// 删除过期消息（保留30天）
        /// </summary>
        public int DeleteExpired()
        {
            string sql = "DELETE FROM Notification WHERE CreateTime < DATE_SUB(NOW(), INTERVAL 30 DAY)";
            return Db.Execute(sql);
        }

        /// <summary>
        /// 检查消息是否已存在（防止重复）
        /// </summary>
        public bool Exists(string receiverID, string senderID, NotificationType type, int contentID)
        {
            string sql = $@"
                SELECT 1 FROM Notification 
                WHERE ReceiverID = ?{nameof(receiverID)} 
                AND SenderID = ?{nameof(senderID)} 
                AND Type = ?type 
                AND ContentID = ?{nameof(contentID)}
                AND CreateTime >= DATE_SUB(NOW(), INTERVAL 1 DAY)";

            return Db.ExecuteScalar<int>(sql, new { receiverID, senderID, type = (int)type, contentID }) > 0;
        }
    }
}
