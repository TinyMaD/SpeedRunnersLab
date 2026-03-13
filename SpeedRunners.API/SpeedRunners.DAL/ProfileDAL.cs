using SpeedRunners.Model.Profile;
using SpeedRunners.Model.Rank;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunners.DAL
{
    public class ProfileDAL : DALBase
    {
        public ProfileDAL(DbHelper db) : base(db) { }

        /// <summary>
        /// 获取玩家基础信息
        /// </summary>
        public MRankInfo GetPlayerInfo(string steamId)
        {
            return Db.QueryFirstOrDefault<MRankInfo>(
                @"SELECT * FROM RankInfo WHERE PlatformID = ?steamId",
                new { steamId });
        }

        /// <summary>
        /// 获取用户隐私设置
        /// </summary>
        public MPrivacySettings GetPrivacySettings(string steamId)
        {
            return Db.QueryFirstOrDefault<MPrivacySettings>(
                $@"SELECT
                     a.PlatformID,
                     CASE WHEN a.State = -1 THEN -1 ELSE 0 END AS State,
                     a.RankType,
                     IFNULL(b.ShowProfile, 1) ShowProfile,
                     IFNULL(b.RequestRankData, 1) RequestRankData,
                     IFNULL(b.ShowAddScore, 1) ShowAddScore,
                     IFNULL(b.ShowWeekPlayTime, 1) ShowWeekPlayTime
                    FROM RankInfo a
                    LEFT JOIN PrivacySettings b ON a.PlatformID = b.PlatformID
                    WHERE a.PlatformID = ?steamId",
                new { steamId });
        }

        /// <summary>
        /// 获取最近两周新增天梯分
        /// </summary>
        public decimal GetPast2WeeksScore(string steamId)
        {
            int dayOfRange = 14;
            var result = Db.ExecuteScalar<decimal?>($@"
                SELECT b.RankScore - a.minScore 
                FROM (
                    SELECT PlatformID, MIN(RankScore) minScore 
                    FROM (
                        SELECT PlatformID, RankScore 
                        FROM RankLog
                        WHERE PlatformID = ?steamId AND Date >= DATE_SUB(CURDATE(), interval {dayOfRange - 1} day)

                        UNION

                        SELECT c.PlatformID, d.RankScore
                        FROM (
                            SELECT PlatformID, MAX(Date) Date
                            FROM RankLog
                            WHERE PlatformID = ?steamId AND Date < DATE_SUB(CURDATE(), interval {dayOfRange - 1} day)
                            GROUP BY PlatformID
                        ) c
                        LEFT JOIN RankLog d ON c.PlatformID = d.PlatformID AND c.Date = d.date
                    ) e 
                    WHERE PlatformID = ?steamId
                    GROUP BY PlatformID
                ) a 
                LEFT JOIN RankInfo b ON a.PlatformID = b.PlatformID
                WHERE a.PlatformID = ?steamId",
                new { steamId });
            
            return result ?? 0;
        }

        /// <summary>
        /// 获取每日天梯分变化记录（用于热度图）
        /// </summary>
        public List<MDailyScore> GetDailyScoreHistory(string steamId)
        {
            // 获取过去365天的每日分数记录
            var logs = Db.Query<MRankLog>($@"
                SELECT PlatformID, RankScore, Date 
                FROM RankLog 
                WHERE PlatformID = ?steamId 
                  AND Date >= DATE_SUB(CURDATE(), INTERVAL 365 DAY)
                ORDER BY Date ASC",
                new { steamId }).ToList();

            if (!logs.Any())
            {
                return new List<MDailyScore>();
            }

            var result = new List<MDailyScore>();
            decimal? prevScore = null;

            // 按日期分组，计算每日增量
            var groupedLogs = logs
                .GroupBy(l => l.Date.Date)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groupedLogs)
            {
                var dayScore = group.OrderByDescending(l => l.Date).First().RankScore;
                var dailyIncrease = 0;
                
                if (prevScore.HasValue)
                {
                    dailyIncrease = (int)(dayScore - prevScore.Value);
                }
                
                result.Add(new MDailyScore
                {
                    Date = group.Key.ToString("yyyy-MM-dd"),
                    Score = Math.Max(0, dailyIncrease) // 只记录正增长
                });
                
                prevScore = dayScore;
            }

            return result;
        }

        /// <summary>
        /// 检查隐私设置（已废弃，请使用 GetPrivacySettings）
        /// </summary>
        [Obsolete("Use GetPrivacySettings instead")]
        public bool CheckPrivacy(string steamId)
        {
            var settings = Db.QueryFirstOrDefault<int?>(
                @"SELECT ShowAddScore FROM PrivacySettings WHERE PlatformID = ?steamId",
                new { steamId });
            
            // 默认公开，ShowAddScore为0时表示公开
            if (settings == null) return true;
            
            return settings == 0;
        }
    }
}
