using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.Rank;
using SpeedRunners.Utils;
using Steam.Models.SteamCommunity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeedRunners.BLL
{
    public class RankBLL : BLLHelper<RankDAL>
    {
        private static SteamBLL _steamBLL;

        public RankBLL(SteamBLL steamBLL)
        {
            _steamBLL = steamBLL;
        }

        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="steamIDs"></param>
        /// <returns></returns>
        public List<MRankInfo> GetRankList(IEnumerable<string> steamIDs = null)
        {
            return BeginDb(DAL =>
            {
                return DAL.GetRankList(steamIDs);
            });
        }

        public List<MParticipateList> GetParticipateList()
        {
            return BeginDb(DAL =>
            {
                var list = DAL.GetRankList(null, false, 1);
                return list.Select(x => new MParticipateList
                {
                    PlatformID = x.PlatformID,
                    PersonaName = x.PersonaName,
                    AvatarM = x.AvatarM,
                    RankScore = x.RankScore,
                    WeekPlayTime = Math.Round((decimal)x.WeekPlayTime / 60, 1),
                    PlayTime = Math.Round((decimal)x.PlayTime / 60, 1),
                    SxlScore = (int)Math.Round((decimal)(x.PlayTime - x.WeekPlayTime) / 6) + (int)x.RankScore
                }).OrderByDescending(x => x.SxlScore).ToList();
            });
        }

        /// <summary>
        /// 获取正在玩SR的玩家
        /// </summary>
        /// <returns></returns>
        public List<MRankInfo> GetPlaySRList()
        {
            return BeginDb(DAL =>
            {
                return DAL.GetRankList(playSROnly: true);
            });
        }

        /// <summary>
        /// 获取新增天梯分排名
        /// </summary>
        /// <returns></returns>
        public List<MRankInfo> GetAddedChart()
        {
            return BeginDb(DAL =>
            {
                return DAL.GetAddedChart();
            });
        }

        /// <summary>
        /// 获取游戏时间排名
        /// </summary>
        /// <returns></returns>
        public List<MRankInfo> GetHourChart()
        {
            return BeginDb(DAL =>
            {
                return DAL.GetHourChart();
            });
        }

        /// <summary>
        /// 初始化用户数据
        /// </summary>
        /// <returns></returns>
        public async Task InitUserData()
        {
            bool exist = true;
            BeginDb(DAL =>
            {
                exist = DAL.ExistRankInfo(CurrentUser.PlatformID);
            });
            // 存在初始数据
            if (exist)
            {
                return;
            }
            // 获取用户信息
            MRankInfo srRankInfo = (await _steamBLL.GetRankScore(new[] { CurrentUser.RankID })).FirstOrDefault();
            PlayerSummaryModel playerSummary = await _steamBLL.GetPlayerSummary(CurrentUser.PlatformID);
            MRankInfo rankInfo = new MRankInfo
            {
                PlatformID = playerSummary.SteamId.ToString(),
                RankID = CurrentUser.RankID,
                PersonaName = playerSummary.Nickname,
                AvatarS = playerSummary.AvatarUrl,
                AvatarM = playerSummary.AvatarMediumUrl,
                AvatarL = playerSummary.AvatarFullUrl,
                State = (int)playerSummary.UserStatus
            };
            RecentlyPlayedGamesResultModel playedGames = await _steamBLL.GetRecentlyPlayedGames(CurrentUser.PlatformID);
            BeginDb(DAL =>
            {
                // 没有SR
                if (srRankInfo == null)
                {
                    DAL.AddRankInfo(rankInfo);
                    return;
                }
                int weekPlayTime = (int)(playedGames.RecentlyPlayedGames.FirstOrDefault(x => x.AppId == 207140)?.Playtime2Weeks ?? 0);
                // 添加RankInfo
                rankInfo.RankType = 1;
                rankInfo.RankCount = srRankInfo.RankCount;
                rankInfo.RankScore = srRankInfo.RankScore;
                rankInfo.OldRankScore = srRankInfo.RankScore;
                rankInfo.RankLevel = srRankInfo.RankLevel;
                rankInfo.WeekPlayTime = weekPlayTime;
                DAL.Db.BeginTrans();
                DAL.AddRankInfo(rankInfo);
                // 添加RankLog
                MRankLog rankLog = new MRankLog
                {
                    PlatformID = CurrentUser.PlatformID,
                    RankScore = srRankInfo.RankScore.Value,
                };
                DAL.AddRankLog(rankLog);
                DAL.Db.CommitTrans();
            });
        }

        /// <summary>
        /// 添加SR基础数据
        /// </summary>
        /// <returns></returns>
        public async Task<MResponse> AsyncSRData()
        {
            MRankInfo srRankInfo = (await _steamBLL.GetRankScore(new[] { CurrentUser.RankID })).FirstOrDefault();
            if (srRankInfo == null)
            {
                return MResponse.Fail("此账号尚未拥有SpeedRunners游戏");
            }
            BeginDb(DAL =>
            {
                srRankInfo.RankType = 1;
                srRankInfo.OldRankScore = srRankInfo.RankScore;
                DAL.UpdateRankInfo(srRankInfo, true);
                if (DAL.ExistRankLog(CurrentUser.PlatformID))
                {
                    return;
                }
                // 添加RankLog
                MRankLog rankLog = new MRankLog
                {
                    PlatformID = CurrentUser.PlatformID,
                    RankScore = srRankInfo.RankScore.Value,
                };
                DAL.AddRankLog(rankLog);
            });
            return MResponse.Success();
        }

        public bool UpdateParticipate(bool participate)
        {
            return BeginDb(DAL =>
            {
                return DAL.UpdateParticipate(CurrentUser?.PlatformID, participate);
            });
        }

        public int GetPrizePool()
        {
            int amount = 1760;
            BeginDb(DAL =>
            {
                amount += DAL.GetPrizePool();
            });
            return amount;
        }
    }
}
