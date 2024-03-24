using SpeedRunners.Model.Rank;
using SpeedRunners.Model.Sponsor;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedRunners.DAL
{
    public class RankDAL : DALBase
    {
        public RankDAL(DbHelper db) : base(db) { }

        #region RankInfo

        public List<MRankInfo> GetAllRankList(IEnumerable<string> steamIDs = null)
        {
            StringBuilder where = new StringBuilder();
            if (steamIDs?.Any() ?? false)
            {
                where.Append($" AND PlatformID IN ?{nameof(steamIDs)} ");
            }
            return Db.Query<MRankInfo>($"SELECT * FROM RankInfo WHERE 1 = 1 {where} ORDER BY RankScore DESC", new { steamIDs }).ToList();
        }

        public List<MRankInfo> GetParticipateList()
        {
            return Db.Query<MRankInfo>($"SELECT * FROM RankInfo WHERE Participate = 1 ORDER BY RankScore DESC").ToList();
        }

        public List<MRankInfo> GetRankList()
        {
            var allList = GetAllRankList();

            return allList.Where(x => x.RankType == 1).ToList();
        }

        public List<MRankInfo> GetPlaySRList()
        {
            return Db.Query<MRankInfo>($"SELECT * FROM RankInfo WHERE GameID = '207140' ORDER BY RankScore DESC").ToList();
        }

        public List<MRankInfo> GetAddedChart()
        {
            int dayOfRange = 14;
            return Db.Query<MRankInfo>($@"SELECT a.PlatformID,
                                                b.PersonaName,
                                                b.RankScore - a.minScore RankScore,
                                                b.AvatarS
                                                FROM (
                                                    SELECT PlatformID, MIN(RankScore) minScore 
                                                    FROM (
                                                        SELECT PlatformID,
                                                                RankScore 
                                                        FROM RankLog
                                                        WHERE Date >= DATE_SUB(CURDATE(), interval {dayOfRange - 1} day)

                                                        UNION

                                                        SELECT c.PlatformID,
                                                                d.RankScore
                                                        FROM (
                                                                SELECT PlatformID,
                                                                        MAX(Date) Date
                                                                FROM RankLog
                                                                WHERE Date < DATE_SUB(CURDATE(), interval {dayOfRange - 1} day)
                                                                GROUP BY PlatformID) c
                                                        LEFT JOIN RankLog d ON c.PlatformID = d.PlatformID AND c.Date = d.date
                                                    ) e 
                                                    GROUP BY PlatformID
                                                ) a 
                                                LEFT JOIN RankInfo b ON a.PlatformID = b.PlatformID
                                                LEFT JOIN PrivacySettings ps ON ps.PlatformID = a.PlatformID
                                                WHERE b.RankScore - a.minScore > 0 
                                                        AND (ISNULL(ps.ShowAddScore) OR ps.ShowAddScore = 1)
                                                ORDER BY RankScore DESC; ").ToList();
        }

        public List<MRankInfo> GetHourChart()
        {
            return Db.Query<MRankInfo>($@"SELECT info.PlatformID, info.PersonaName, info.WeekPlayTime, info.AvatarS 
                                            FROM RankInfo info
                                            LEFT JOIN PrivacySettings ps ON ps.PlatformID = info.PlatformID
                                            WHERE info.WeekPlayTime > 0 AND (ISNULL(ps.ShowWeekPlayTime) OR ps.ShowWeekPlayTime = 1)
                                            ORDER BY info.WeekPlayTime DESC").ToList();
        }

        public bool ExistRankInfo(string steamID)
        {
            return Db.ExecuteScalar<int>($"SELECT 1 FROM RankInfo WHERE PlatformID = ?{nameof(steamID)}", new { steamID }) > 0;
        }

        /// <summary>
        /// 更新RankInfo
        /// </summary>
        /// <param name="rankInfo"></param>
        /// <param name="SRInfoOnly">是否只更新SR信息的列</param>
        public void UpdateRankInfo(MRankInfo rankInfo, bool SRInfoOnly = false)
        {
            string cols = string.Empty;
            if (!SRInfoOnly)
            {
                // 加入其它列
                cols = $"";
            }
            Db.Execute($"UPDATE RankInfo SET RankType = ?{nameof(rankInfo.RankType)}, RankCount = ?{nameof(rankInfo.RankCount)}, RankScore = ?{nameof(rankInfo.RankScore)}, OldRankScore = ?{nameof(rankInfo.OldRankScore)}, RankLevel = ?{nameof(rankInfo.RankLevel)} {cols}  WHERE PlatformID = ?{nameof(rankInfo.PlatformID)}", rankInfo);
        }

        public void AddRankInfo(MRankInfo rankInfo)
        {
            rankInfo.CreateTime = DateTime.Now;
            rankInfo.ModifyTime = DateTime.Now;
            Db.Insert("RankInfo", rankInfo);
        }

        public bool UpdateParticipate(string CurrentUserID, bool participate)
        {
            return Db.Execute($"UPDATE RankInfo SET participate = {(participate ? 1 : 0)}  WHERE PlatformID = {CurrentUserID}") > 0;
        }

        #endregion

        #region RankLog
        public bool ExistRankLog(string steamID)
        {
            return Db.ExecuteScalar<int>($"SELECT 1 FROM RankLog WHERE PlatformID = ?{nameof(steamID)}", new { steamID }) > 0;
        }

        public void AddRankLog(MRankLog rankLog)
        {
            rankLog.Date = DateTime.Now;
            Db.Insert("RankLog", rankLog);
        }
        #endregion

        public List<MSponsor> GetSponsor()
        {
            return Db.Query<MSponsor>($"SELECT * FROM Sponsor WHERE MatchNo = 102 ORDER BY Amount DESC").ToList();
        }
    }
}
