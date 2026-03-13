using SpeedRunners.DAL;
using SpeedRunners.Model.Profile;
using SpeedRunners.Model.Rank;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeedRunners.BLL
{
    public class ProfileBLL : BLLHelper<ProfileDAL>
    {
        private readonly SteamBLL _steamBLL;
        private readonly AchievementSchemaService _achievementSchemaService;

        public ProfileBLL(SteamBLL steamBLL, AchievementSchemaService achievementSchemaService)
        {
            _steamBLL = steamBLL;
            _achievementSchemaService = achievementSchemaService;
        }

        /// <summary>
        /// 获取个人主页数据
        /// </summary>
        /// <param name="steamId">被访问用户的Steam ID</param>
        /// <param name="visitorId">访问者的Steam ID（未登录时为null）</param>
        public async Task<MProfileData> GetProfileData(string steamId, string visitorId = null)
        {
            // 从数据库获取基础信息
            MRankInfo playerInfo = BeginDb(DAL => DAL.GetPlayerInfo(steamId));
            
            if (playerInfo == null)
            {
                // 尝试从Steam API获取
                try
                {
                    var steamPlayer = await _steamBLL.GetPlayerSummary(steamId);
                    if (steamPlayer == null) return null;
                    
                    return new MProfileData
                    {
                        PlatformID = steamId,
                        PersonaName = steamPlayer.Nickname,
                        AvatarS = steamPlayer.AvatarUrl,
                        AvatarM = steamPlayer.AvatarMediumUrl,
                        AvatarL = steamPlayer.AvatarFullUrl,
                        State = (int)steamPlayer.UserStatus,
                        GameID = steamPlayer.PlayingGameId,
                        RankLevel = 0,
                        RankScore = null,
                        TotalPlaytime = 0,
                        Past2WeeksPlaytime = 0,
                        Past2WeeksScore = 0,
                        Stats = new List<MGameStat>(),
                        IsPrivate = false
                    };
                }
                catch
                {
                    return null;
                }
            }

            // 判断是否本人
            bool isOwner = !string.IsNullOrEmpty(visitorId) && steamId == visitorId;

            // 获取隐私设置
            MPrivacySettings privacySettings = BeginDb(DAL => DAL.GetPrivacySettings(steamId));

            // 本人访问：返回完整数据
            if (isOwner)
            {
                return BuildFullProfileData(playerInfo, privacySettings);
            }

            // 非本人访问：检查个人主页开关
            if (privacySettings.ShowProfile == 0)
            {
                // 主页关闭：返回最小化数据
                return BuildMinimalProfileData(playerInfo);
            }

            // 主页开启：根据隐私设置过滤数据
            return BuildFilteredProfileData(playerInfo, privacySettings);
        }

        /// <summary>
        /// 构建完整数据（本人访问）
        /// </summary>
        private MProfileData BuildFullProfileData(MRankInfo playerInfo, MPrivacySettings privacySettings)
        {
            decimal past2WeeksScore = BeginDb(DAL => DAL.GetPast2WeeksScore(playerInfo.PlatformID));
            decimal totalPlaytime = Math.Round((decimal)playerInfo.PlayTime / 60, 1);
            decimal past2WeeksPlaytime = Math.Round((decimal)playerInfo.WeekPlayTime / 60, 1);

            return new MProfileData
            {
                PlatformID = playerInfo.PlatformID,
                PersonaName = playerInfo.PersonaName,
                AvatarS = playerInfo.AvatarS,
                AvatarM = playerInfo.AvatarM,
                AvatarL = playerInfo.AvatarL,
                State = playerInfo.State,
                GameID = playerInfo.GameID,
                RankLevel = playerInfo.RankLevel,
                RankScore = playerInfo.RankScore,
                TotalPlaytime = totalPlaytime,
                Past2WeeksPlaytime = past2WeeksPlaytime,
                Past2WeeksScore = past2WeeksScore,
                Stats = BuildGameStats(playerInfo, null), // 本人访问不过滤
                IsPrivate = false
            };
        }

        /// <summary>
        /// 构建最小化数据（主页关闭）
        /// </summary>
        private MProfileData BuildMinimalProfileData(MRankInfo playerInfo)
        {
            return new MProfileData
            {
                PlatformID = playerInfo.PlatformID,
                PersonaName = playerInfo.PersonaName,
                AvatarS = playerInfo.AvatarS,
                AvatarM = playerInfo.AvatarM,
                AvatarL = playerInfo.AvatarL,
                State = 0, // 隐藏在线状态
                GameID = null, // 隐藏当前游戏
                RankLevel = 0,
                RankScore = null,
                TotalPlaytime = 0,
                Past2WeeksPlaytime = 0,
                Past2WeeksScore = 0,
                Stats = new List<MGameStat>(),
                IsPrivate = true
            };
        }

        /// <summary>
        /// 构建过滤后的数据（根据隐私设置）
        /// </summary>
        private MProfileData BuildFilteredProfileData(MRankInfo playerInfo, MPrivacySettings privacySettings)
        {
            decimal totalPlaytime = Math.Round((decimal)playerInfo.PlayTime / 60, 1);
            decimal past2WeeksPlaytime = 0;
            decimal past2WeeksScore = 0;

            // ShowWeekPlayTime 控制最近两周游戏时长
            if (privacySettings.ShowWeekPlayTime == 1)
            {
                past2WeeksPlaytime = Math.Round((decimal)playerInfo.WeekPlayTime / 60, 1);
            }

            // ShowAddScore 控制最近两周新增天梯分
            if (privacySettings.ShowAddScore == 1)
            {
                past2WeeksScore = BeginDb(DAL => DAL.GetPast2WeeksScore(playerInfo.PlatformID));
            }

            // State 控制在线状态和当前游戏
            int state = playerInfo.State;
            string gameId = playerInfo.GameID;
            if (privacySettings.State == -1)
            {
                state = 0; // 显示为离线
                gameId = null;
            }

            // RequestRankData 控制天梯分、段位、排位场次
            decimal? rankScore = playerInfo.RankScore;
            int rankLevel = playerInfo.RankLevel;
            if (privacySettings.RequestRankData == 0)
            {
                rankScore = null;
                rankLevel = 0;
            }

            return new MProfileData
            {
                PlatformID = playerInfo.PlatformID,
                PersonaName = playerInfo.PersonaName,
                AvatarS = playerInfo.AvatarS,
                AvatarM = playerInfo.AvatarM,
                AvatarL = playerInfo.AvatarL,
                State = state,
                GameID = gameId,
                RankLevel = rankLevel,
                RankScore = rankScore,
                TotalPlaytime = totalPlaytime,
                Past2WeeksPlaytime = past2WeeksPlaytime,
                Past2WeeksScore = past2WeeksScore,
                Stats = BuildGameStats(playerInfo, privacySettings),
                IsPrivate = false
            };
        }

        /// <summary>
        /// 获取每日天梯分历史记录
        /// </summary>
        public List<MDailyScore> GetDailyScoreHistory(string steamId, string visitorId = null)
        {
            // 判断是否本人
            bool isOwner = !string.IsNullOrEmpty(visitorId) && steamId == visitorId;

            // 本人访问：返回完整数据
            if (isOwner)
            {
                return BeginDb(DAL => DAL.GetDailyScoreHistory(steamId));
            }

            // 非本人访问：检查隐私设置
            MPrivacySettings privacySettings = BeginDb(DAL => DAL.GetPrivacySettings(steamId));

            // 主页关闭或禁止获取天梯分：返回空列表
            if (privacySettings.ShowProfile == 0 || privacySettings.RequestRankData == 0)
            {
                return new List<MDailyScore>();
            }

            return BeginDb(DAL => DAL.GetDailyScoreHistory(steamId));
        }

        /// <summary>
        /// 获取玩家成就
        /// </summary>
        public async Task<List<MAchievement>> GetAchievements(string steamId, string visitorId = null)
        {
            // 判断是否本人
            bool isOwner = !string.IsNullOrEmpty(visitorId) && steamId == visitorId;

            // 非本人访问：检查隐私设置
            if (!isOwner)
            {
                MPrivacySettings privacySettings = BeginDb(DAL => DAL.GetPrivacySettings(steamId));
                
                // 主页关闭：返回空列表
                if (privacySettings.ShowProfile == 0)
                {
                    return new List<MAchievement>();
                }
            }

            // 从缓存服务获取SpeedRunners游戏的成就定义
            var achievementSchemas = await _achievementSchemaService.GetAchievementSchemaAsync();
            
            if (achievementSchemas == null || !achievementSchemas.Any())
            {
                return new List<MAchievement>();
            }

            // 转换为成就列表
            var achievements = achievementSchemas.Select(schema => new MAchievement
            {
                ApiName = schema.ApiName,
                Name = schema.DisplayName,
                Description = schema.Hidden && string.IsNullOrEmpty(schema.Description) 
                    ? Localizer["hiddenAchievement"] 
                    : schema.Description,
                IconUrl = schema.Icon,
                IconGrayUrl = schema.IconGray,
                Hidden = schema.Hidden,
                Unlocked = false
            }).ToList();
            
            try
            {
                // 从Steam获取玩家成就状态
                var playerAchievements = await _steamBLL.GetPlayerAchievements(steamId);
                
                if (playerAchievements?.Any() == true)
                {
                    foreach (var ach in achievements)
                    {
                        var playerAch = playerAchievements.FirstOrDefault(a => a.ApiName == ach.ApiName);
                        if (playerAch != null)
                        {
                            ach.Unlocked = playerAch.Achieved == 1;
                            ach.UnlockedAt = playerAch.UnlockTime;
                        }
                    }
                }
            }
            catch
            {
                // Steam API调用失败时返回成就列表（全部未解锁状态）
            }

            // 按解锁状态排序：已解锁的在前，已解锁按达成时间正序，未解锁按名称排序
            return achievements
                .OrderBy(a => a.Unlocked ? 0 : 1)
                .ThenBy(a => a.UnlockedAt ?? DateTime.MaxValue)
                .ThenBy(a => a.Name)
                .ToList();
        }

        /// <summary>
        /// 构建游戏统计数据
        /// </summary>
        /// <param name="playerInfo">玩家信息</param>
        /// <param name="privacySettings">隐私设置（本人访问时为null）</param>
        private List<MGameStat> BuildGameStats(MRankInfo playerInfo, MPrivacySettings privacySettings)
        {
            var stats = new List<MGameStat>();
            bool isOwner = privacySettings == null;

            // 段位（受 RequestRankData 控制）
            if (isOwner || privacySettings.RequestRankData == 1)
            {
                string rankName = GetRankName(playerInfo.RankLevel, Localizer);
                stats.Add(new MGameStat { Name = Localizer["rank"], Value = rankName });
            }

            // 天梯分已移除，统一从 MProfileData.RankScore 获取

            // 排位场次（受 RequestRankData 控制）
            if (isOwner || privacySettings.RequestRankData == 1)
            {
                if (playerInfo.RankCount.HasValue)
                {
                    stats.Add(new MGameStat { Name = Localizer["rankCount"], Value = playerInfo.RankCount.Value.ToString() });
                }
            }

            // 总游戏时长（始终显示）
            var totalHours = Math.Round((decimal)playerInfo.PlayTime / 60, 1);
            stats.Add(new MGameStat { Name = Localizer["totalPlaytime"], Value = $"{totalHours} {Localizer["hours"]}" });

            // 最近两周游戏时长（受 ShowWeekPlayTime 控制）
            if (isOwner || privacySettings.ShowWeekPlayTime == 1)
            {
                var weekHours = Math.Round((decimal)playerInfo.WeekPlayTime / 60, 1);
                stats.Add(new MGameStat { Name = Localizer["recentPlaytime"], Value = $"{weekHours} {Localizer["hours"]}" });
            }

            return stats;
        }

        /// <summary>
        /// 获取段位名称
        /// </summary>
        private string GetRankName(int rankLevel, Microsoft.Extensions.Localization.IStringLocalizer localizer)
        {
            return rankLevel switch
            {
                0 => localizer["rank_entry"],
                1 => localizer["rank_beginner"],
                2 => localizer["rank_advanced"],
                3 => localizer["rank_expert"],
                4 => localizer["rank_bronze"],
                5 => localizer["rank_silver"],
                6 => localizer["rank_gold"],
                7 => localizer["rank_platinum"],
                8 => localizer["rank_diamond"],
                9 => localizer["rank_kos"],
                _ => localizer["rank_unknown"]
            };
        }
    }
}
