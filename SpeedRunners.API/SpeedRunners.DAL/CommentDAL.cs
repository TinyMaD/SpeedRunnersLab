using SpeedRunners.Model;
using SpeedRunners.Model.Comment;
using SpeedRunners.Utils;
using System.Linq;
using static Dapper.SqlMapper;

namespace SpeedRunners.DAL
{
    public class CommentDAL : DALBase
    {
        public CommentDAL(DbHelper db) : base(db) { }

        /// <summary>
        /// 获取顶级评论列表（分页）
        /// </summary>
        public MPageResult<MCommentOut> GetCommentList(MCommentPageParam param)
        {
            GridReader reader = Db.QueryMultiple($@"
                SELECT COUNT(c.ID)
                FROM `Comment` c
                WHERE c.PagePath = ?{nameof(param.PagePath)}
                  AND c.ParentID IS NULL
                  AND c.IsDeleted = 0;

                SELECT c.ID, c.PagePath, c.PlatformID, c.ParentID, c.ReplyToPlatformID,
                       c.Content, c.CreateTime, c.IsDeleted,
                       IFNULL(r.PersonaName, c.PlatformID) AS PersonaName,
                       r.AvatarS,
                       (SELECT COUNT(*) FROM `Comment` sub WHERE sub.ParentID = c.ID AND sub.IsDeleted = 0) AS ReplyCount,
                       (SELECT COUNT(*) FROM `CommentLike` cl WHERE cl.CommentID = c.ID) AS LikeCount,
                       IF(?{nameof(param.CurrentUserPlatformID)} IS NOT NULL AND EXISTS(SELECT 1 FROM `CommentLike` cl2 WHERE cl2.CommentID = c.ID AND cl2.PlatformID = ?{nameof(param.CurrentUserPlatformID)}), 1, 0) AS IsLiked
                FROM `Comment` c
                LEFT JOIN `RankInfo` r ON c.PlatformID = r.PlatformID
                WHERE c.PagePath = ?{nameof(param.PagePath)}
                  AND c.ParentID IS NULL
                  AND c.IsDeleted = 0
                ORDER BY c.CreateTime DESC
                LIMIT ?{nameof(param.Offset)}, ?{nameof(param.PageSize)};", param);

            return new MPageResult<MCommentOut>
            {
                Total = reader.ReadFirst<int>(),
                List = reader.Read<MCommentOut>()
            };
        }

        /// <summary>
        /// 获取回复列表（分页）
        /// </summary>
        public MPageResult<MCommentOut> GetReplyList(MCommentPageParam param)
        {
            GridReader reader = Db.QueryMultiple($@"
                SELECT COUNT(c.ID)
                FROM `Comment` c
                WHERE c.ParentID = ?{nameof(param.ParentID)}
                  AND c.IsDeleted = 0;

                SELECT c.ID, c.PagePath, c.PlatformID, c.ParentID, c.ReplyToPlatformID,
                       c.Content, c.CreateTime, c.IsDeleted,
                       IFNULL(r.PersonaName, c.PlatformID) AS PersonaName,
                       r.AvatarS,
                       IFNULL(r2.PersonaName, c.ReplyToPlatformID) AS ReplyToPersonaName,
                       (SELECT COUNT(*) FROM `CommentLike` cl WHERE cl.CommentID = c.ID) AS LikeCount,
                       IF(?{nameof(param.CurrentUserPlatformID)} IS NOT NULL AND EXISTS(SELECT 1 FROM `CommentLike` cl2 WHERE cl2.CommentID = c.ID AND cl2.PlatformID = ?{nameof(param.CurrentUserPlatformID)}), 1, 0) AS IsLiked
                FROM `Comment` c
                LEFT JOIN `RankInfo` r ON c.PlatformID = r.PlatformID
                LEFT JOIN `RankInfo` r2 ON c.ReplyToPlatformID = r2.PlatformID
                WHERE c.ParentID = ?{nameof(param.ParentID)}
                  AND c.IsDeleted = 0
                ORDER BY c.CreateTime ASC
                LIMIT ?{nameof(param.Offset)}, ?{nameof(param.PageSize)};", param);

            return new MPageResult<MCommentOut>
            {
                Total = reader.ReadFirst<int>(),
                List = reader.Read<MCommentOut>()
            };
        }

        /// <summary>
        /// 添加评论
        /// </summary>
        public int AddComment(MComment param)
        {
            return Db.Insert("Comment", param, new[] { nameof(param.ID), nameof(param.IsDeleted) });
        }

        /// <summary>
        /// 获取单条评论
        /// </summary>
        public MComment GetComment(int commentID)
        {
            return Db.QueryFirstOrDefault<MComment>(
                $"SELECT * FROM `Comment` WHERE `ID` = ?{nameof(commentID)} AND IsDeleted = 0",
                new { commentID });
        }

        /// <summary>
        /// 软删除评论
        /// </summary>
        public void DeleteComment(int commentID)
        {
            Db.Execute(
                $"UPDATE `Comment` SET IsDeleted = 1 WHERE `ID` = ?{nameof(commentID)}",
                new { commentID });
        }

        /// <summary>
        /// 软删除某条评论的所有回复
        /// </summary>
        public void DeleteReplies(int parentID)
        {
            Db.Execute(
                $"UPDATE `Comment` SET IsDeleted = 1 WHERE `ParentID` = ?{nameof(parentID)}",
                new { parentID });
        }

        /// <summary>
        /// 切换点赞状态，返回操作后的点赞数，并通过 out 参数返回是否是点赞操作
        /// </summary>
        public int ToggleLike(int commentID, string platformID, out bool isLiked)
        {
            var existing = Db.QueryFirstOrDefault<MCommentLike>(
                $"SELECT * FROM `CommentLike` WHERE CommentID = ?{nameof(commentID)} AND PlatformID = ?{nameof(platformID)}",
                new { commentID, platformID });

            if (existing != null)
            {
                // 取消点赞
                Db.Execute(
                    $"DELETE FROM `CommentLike` WHERE CommentID = ?{nameof(commentID)} AND PlatformID = ?{nameof(platformID)}",
                    new { commentID, platformID });
                isLiked = false;
            }
            else
            {
                // 点赞
                Db.Execute(
                    $"INSERT INTO `CommentLike` (CommentID, PlatformID) VALUES (?{nameof(commentID)}, ?{nameof(platformID)})",
                    new { commentID, platformID });
                isLiked = true;
            }

            return Db.QueryFirst<int>(
                $"SELECT COUNT(*) FROM `CommentLike` WHERE CommentID = ?{nameof(commentID)}",
                new { commentID });
        }
    }
}
