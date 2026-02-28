using SpeedRunners.DAL;
using SpeedRunners.Model.Profile;
using SpeedRunners.Model.Rank;
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

        public ProfileBLL(SteamBLL steamBLL)
        {
            _steamBLL = steamBLL;
        }

        /// <summary>
        /// 获取个人主页数据
        /// </summary>
        public async Task<MProfileData> GetProfileData(string steamId)
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
                        RankScore = 0,
                        TotalPlaytime = 0,
                        Past2WeeksPlaytime = 0,
                        Past2WeeksScore = 0,
                        Stats = new List<MGameStat>()
                    };
                }
                catch
                {
                    return null;
                }
            }

            // 检查隐私设置
            bool isPublic = BeginDb(DAL => DAL.CheckPrivacy(steamId));

            // 获取最近两周新增分数
            decimal past2WeeksScore = 0;
            if (isPublic)
            {
                past2WeeksScore = BeginDb(DAL => DAL.GetPast2WeeksScore(steamId));
            }

            // 计算游戏时长（分钟转小时）
            decimal totalPlaytime = Math.Round((decimal)playerInfo.PlayTime / 60, 1);
            decimal past2WeeksPlaytime = Math.Round((decimal)playerInfo.WeekPlayTime / 60, 1);

            // 构建游戏统计
            var stats = BuildGameStats(playerInfo, Localizer);

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
                RankScore = isPublic ? playerInfo.RankScore : null,
                TotalPlaytime = totalPlaytime,
                Past2WeeksPlaytime = past2WeeksPlaytime,
                Past2WeeksScore = past2WeeksScore,
                Stats = stats
            };
        }

        /// <summary>
        /// 获取每日天梯分历史记录
        /// </summary>
        public List<MDailyScore> GetDailyScoreHistory(string steamId)
        {
            // 检查隐私设置
            bool isPublic = BeginDb(DAL => DAL.CheckPrivacy(steamId));
            if (!isPublic)
            {
                return new List<MDailyScore>();
            }

            return BeginDb(DAL => DAL.GetDailyScoreHistory(steamId));
        }

        /// <summary>
        /// 获取玩家成就
        /// </summary>
        public async Task<List<MAchievement>> GetAchievements(string steamId)
        {
            // SpeedRunners成就定义
            var achievementDefs = GetSpeedRunnersAchievements();
            
            try
            {
                // 从Steam获取玩家成就状态
                var playerAchievements = await _steamBLL.GetPlayerAchievements(steamId);
                
                if (playerAchievements?.Any() == true)
                {
                    foreach (var def in achievementDefs)
                    {
                        var playerAch = playerAchievements.FirstOrDefault(a => a.ApiName == def.ApiName);
                        if (playerAch != null)
                        {
                            def.Unlocked = playerAch.Achieved == 1;
                            def.UnlockedAt = playerAch.UnlockTime;
                        }
                    }
                }
            }
            catch
            {
                // Steam API调用失败时返回默认列表
            }

            return achievementDefs;
        }

        /// <summary>
        /// 构建游戏统计数据
        /// </summary>
        private List<MGameStat> BuildGameStats(MRankInfo playerInfo, Microsoft.Extensions.Localization.IStringLocalizer localizer)
        {
            var stats = new List<MGameStat>();

            // 段位
            string rankName = GetRankName(playerInfo.RankLevel);
            stats.Add(new MGameStat { Name = "段位", Value = rankName });

            // 天梯分
            if (playerInfo.RankScore.HasValue)
            {
                stats.Add(new MGameStat { Name = "天梯分", Value = playerInfo.RankScore.Value.ToString("N0") });
            }

            // 排位场次
            if (playerInfo.RankCount.HasValue)
            {
                stats.Add(new MGameStat { Name = "排位场次", Value = playerInfo.RankCount.Value.ToString() });
            }

            // 总游戏时长
            var totalHours = Math.Round((decimal)playerInfo.PlayTime / 60, 1);
            stats.Add(new MGameStat { Name = "总游戏时长", Value = $"{totalHours} 小时" });

            // 最近两周游戏时长
            var weekHours = Math.Round((decimal)playerInfo.WeekPlayTime / 60, 1);
            stats.Add(new MGameStat { Name = "最近两周", Value = $"{weekHours} 小时" });

            return stats;
        }

        /// <summary>
        /// 获取段位名称
        /// </summary>
        private string GetRankName(int rankLevel)
        {
            return rankLevel switch
            {
                0 => "新手",
                1 => "入门",
                2 => "进阶",
                3 => "专家",
                4 => "青铜",
                5 => "白银",
                6 => "黄金",
                7 => "铂金",
                8 => "钻石",
                9 => "KOS",
                _ => "未知"
            };
        }

        /// <summary>
        /// SpeedRunners游戏成就列表
        /// </summary>
        private List<MAchievement> GetSpeedRunnersAchievements()
        {
            return new List<MAchievement>
            {
                new MAchievement { Id = 1, ApiName = "FIRST_BLOOD", Name = "首胜", Description = "赢得第一场比赛", Icon = "mdi-trophy" },
                new MAchievement { Id = 2, ApiName = "WIN_10", Name = "连胜达人", Description = "赢得10场比赛", Icon = "mdi-trophy-variant" },
                new MAchievement { Id = 3, ApiName = "WIN_100", Name = "百胜将军", Description = "赢得100场比赛", Icon = "mdi-trophy-award" },
                new MAchievement { Id = 4, ApiName = "RANKED_BRONZE", Name = "青铜之路", Description = "达到青铜段位", Icon = "mdi-medal" },
                new MAchievement { Id = 5, ApiName = "RANKED_SILVER", Name = "白银荣耀", Description = "达到白银段位", Icon = "mdi-medal-outline" },
                new MAchievement { Id = 6, ApiName = "RANKED_GOLD", Name = "黄金时代", Description = "达到黄金段位", Icon = "mdi-star" },
                new MAchievement { Id = 7, ApiName = "RANKED_PLATINUM", Name = "铂金传说", Description = "达到铂金段位", Icon = "mdi-star-outline" },
                new MAchievement { Id = 8, ApiName = "RANKED_DIAMOND", Name = "钻石之巅", Description = "达到钻石段位", Icon = "mdi-diamond-stone" },
                new MAchievement { Id = 9, ApiName = "PLAY_100H", Name = "百小时老兵", Description = "游戏时长达到100小时", Icon = "mdi-clock" },
                new MAchievement { Id = 10, ApiName = "PLAY_500H", Name = "资深玩家", Description = "游戏时长达到500小时", Icon = "mdi-clock-check" },
            };
        }
    }
}
