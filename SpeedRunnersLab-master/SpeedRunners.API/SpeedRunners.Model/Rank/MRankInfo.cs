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
        public int State { get; set; }
        public string GameID { get; set; }
        public int RankLevel { get; set; }
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
