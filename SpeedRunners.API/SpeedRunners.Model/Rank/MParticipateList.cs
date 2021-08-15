using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Model.Rank
{
    public class MParticipateList
    {
        public string PlatformID { get; set; }
        public string PersonaName { get; set; }
        public string AvatarM { get; set; }
        public decimal? RankScore { get; set; }
        public decimal WeekPlayTime { get; set; }
        public decimal PlayTime { get; set; }
        public int SxlScore { get; set; }
    }
}
