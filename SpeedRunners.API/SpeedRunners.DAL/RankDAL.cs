using SpeedRunners.Model.Rank;
using SpeedRunners.Model.Sponsor;
using SpeedRunners.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedRunners.DAL
{
    public class RankDAL : DALBase
    {
        public RankDAL(DbHelper db) : base(db) { }

        #region RankInfo
        public List<MRankInfo> GetRankList(IEnumerable<string> steamIDs = null, bool playSROnly = false, int participate = 0)
        {
            StringBuilder where = new StringBuilder();
            if (steamIDs?.Any() ?? false)
            {
                where.Append($" AND PlatformID IN @{nameof(steamIDs)} ");
            }
            if (playSROnly)
            {
                where.Append(" AND GameID = 207140 ");
            }
            if (participate != 0)
            {
                where.Append($" AND participate = {participate} ");
            }
            return Db.Query<MRankInfo>($"select * from RankInfo where RankType = 1 {where} order by RankScore desc", new { steamIDs }).ToList();
        }

        public List<MRankInfo> GetAddedChart()
        {
            int dayOfRange = 14;
            return Db.Query<MRankInfo>($@"select a.PlatformID, b.PersonaName, b.RankScore - a.minScore RankScore, b.AvatarS from 
(
    select PlatformID, min(RankScore) minScore from
    (
        select PlatformID, RankScore from RankLog where Date >= getdate() - {dayOfRange - 1}
        union
        select c.PlatformID, d.RankScore from(select PlatformID, max(Date) Date from RankLog where Date < getdate() - {dayOfRange - 1} group by PlatformID)c
        left join RankLog d on c.PlatformID = d.PlatformID and c.Date = d.date
    )e group by PlatformID
)a left join RankInfo b on a.PlatformID = b.PlatformID
where b.RankScore - a.minScore > 0 order by RankScore desc; ").ToList();
        }

        public List<MRankInfo> GetHourChart()
        {
            return Db.Query<MRankInfo>(@"select PlatformID, PersonaName, WeekPlayTime, AvatarS from RankInfo where WeekPlayTime > 0 order by WeekPlayTime desc").ToList();
        }

        public bool ExistRankInfo(string steamID)
        {
            return Db.ExecuteScalar<int>($"SELECT 1 FROM RankInfo WHERE PlatformID = @{nameof(steamID)}", new { steamID }) > 0;
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
            Db.Execute($"UPDATE RankInfo SET RankType = @{nameof(rankInfo.RankType)}, RankCount = @{nameof(rankInfo.RankCount)}, RankScore = @{nameof(rankInfo.RankScore)}, OldRankScore = @{nameof(rankInfo.OldRankScore)}, RankLevel = @{nameof(rankInfo.RankLevel)} {cols}  WHERE PlatformID = @{nameof(rankInfo.PlatformID)}", rankInfo);
        }

        public void AddRankInfo(MRankInfo rankInfo)
        {
            Db.Insert("RankInfo", rankInfo, new[] { nameof(rankInfo.CreateTime), nameof(rankInfo.ModifyTime) });
        }

        public bool UpdateParticipate(string CurrentUserID, bool participate)
        {
            return Db.Execute($"UPDATE RankInfo SET participate = {(participate ? 1 : 0)}  WHERE PlatformID = {CurrentUserID}") > 0;
        }

        #endregion

        #region RankLog
        public bool ExistRankLog(string steamID)
        {
            return Db.ExecuteScalar<int>($"SELECT 1 FROM RankLog WHERE PlatformID = @{nameof(steamID)}", new { steamID }) > 0;
        }

        public void AddRankLog(MRankLog rankLog)
        {
            Db.Insert("RankLog", rankLog, new[] { nameof(rankLog.Date) });
        }
        #endregion

        public List<MSponsor> GetSponsor()
        {
            return Db.Query<MSponsor>($"SELECT Name, Amount FROM sponsor WHERE MatchNo = 102 ORDER BY Amount DESC").ToList();
        }
    }
}
