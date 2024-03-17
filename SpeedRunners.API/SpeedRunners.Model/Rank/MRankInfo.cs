using System;

namespace SpeedRunners.Model.Rank
{
    public class MRankInfo
    {
        public string PlatformID { get; set; }
        public string RankID { get; set; }
        public int? TmdID { get; set; }
        public string PersonaName { get; set; }
        public string AvatarS { get; set; }
        public string AvatarM { get; set; }
        public string AvatarL { get; set; }

        /// <summary>
        /// -1隐私权限, 0离线, 1在线, 2忙碌, 3离开, 4打盹, 5 想交易, 6想玩游戏
        /// </summary>
        public int State { get; set; }
        public string GameID { get; set; }
        public int RankLevel { get; set; }

        /// <summary>
        /// 参与状态;0无游戏,1上榜,2隐私权限不上榜，3资料隐私
        /// </summary>
        public int RankType { get; set; }
        public int? RankCount { get; set; }
        public decimal? RankScore { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifyTime { get; set; }
        public decimal? OldRankScore { get; set; }
        public int WeekPlayTime { get; set; }
        public int PlayTime { get; set; }
        public int Participate { get; set; }
    }
}
