using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.Rank;
using SpeedRunners.Model.Sponsor;
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
        public List<MRankInfo> GetRankList()
        {
            return BeginDb(DAL =>
            {
                return DAL.GetRankList();
            });
        }

        public List<MRankInfo> GetAllRankList(IEnumerable<string> steamIDs = null)
        {
            return BeginDb(DAL =>
            {
                return DAL.GetAllRankList(steamIDs);
            });
        }

        public List<MParticipateList> GetParticipateList()
        {
            return BeginDb(DAL =>
            {
                var list = DAL.GetParticipateList();
                return list.Select(x => new MParticipateList
                {
                    PlatformID = x.PlatformID,
                    PersonaName = x.PersonaName,
                    AvatarM = x.AvatarM,
                    RankScore = x.RankScore,
                    WeekPlayTime = Math.Round((decimal)x.WeekPlayTime / 60, 1),
                    PlayTime = Math.Round((decimal)x.PlayTime / 60, 1),
                    SxlScore = (x.PlayTime - x.WeekPlayTime > 60000 ? 10000 : (int)Math.Round((decimal)(x.PlayTime - x.WeekPlayTime) / 6)) + (int)x.RankScore
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
                return DAL.GetPlaySRList();
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
            int? rankType = null;
            BeginDb(DAL =>
            {
                rankType = DAL.GetRankType(CurrentUser.PlatformID);
            });
            // 已经有 SR 数据（RankType 1/2/3），不需要重复初始化
            if (rankType.HasValue && rankType.Value != 0)
            {
                return;
            }
            // 之前登录时还没买 SR 留下了空壳行，尝试补齐 SR 数据
            if (rankType.HasValue)
            {
                await AsyncSRData();
                return;
            }
            // 首次登录，拉 Steam 资料建行
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
                DAL.Db.BeginTrans();
                // 拉 Steam 这段时间里可能有另一次并发 InitUserData 已经把行建了，重检防止重复插入
                if (DAL.GetRankType(CurrentUser.PlatformID).HasValue)
                {
                    DAL.Db.RollbackTrans();
                    return;
                }
                // 没有SR
                if (srRankInfo == null)
                {
                    DAL.AddRankInfo(rankInfo);
                    DAL.Db.CommitTrans();
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
                bool hasSR = (await _steamBLL.GetOwnedGames(CurrentUser.PlatformID))?.OwnedGames?.Any(x => x.AppId == 207140) ?? false;
                if (!hasSR) return MResponse.Fail("此账号尚未拥有SpeedRunners游戏");
                // Steam 天梯接口暂时拿不到数据：只在库里还是空壳行（RankType=0）时写空壳，
                // 已有真实数据（RankType=1/2/3）则保留原值，避免被擦掉
                int? currentRankType = null;
                BeginDb(DAL =>
                {
                    currentRankType = DAL.GetRankType(CurrentUser.PlatformID);
                });
                if (currentRankType.GetValueOrDefault() != 0)
                {
                    return MResponse.Success();
                }
                srRankInfo = new MRankInfo { RankType = 0 };
            }
            else
            {
                srRankInfo.RankType = 1;
                srRankInfo.OldRankScore = srRankInfo.RankScore;
            }
            srRankInfo.PlatformID = CurrentUser.PlatformID;
            BeginDb(DAL =>
            {
                DAL.UpdateRankInfo(srRankInfo, true);
                if (!srRankInfo.RankScore.HasValue || DAL.ExistRankLog(CurrentUser.PlatformID))
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

        public List<MSponsor> GetSponsor()
        {
            return BeginDb(DAL =>
            {
                return DAL.GetSponsor();
            });
        }
    }
}
