using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Model.User
{
    public class MPrivacySettings
    {
        public string PlatformID { get; set; }
        /// <summary>
        /// 关state：-1；开0
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 个人主页开关：0关闭，1开启（默认）
        /// </summary>
        public int ShowProfile { get; set; }
        public int ShowWeekPlayTime { get; set; }
        public int RequestRankData { get; set; }
        public int ShowAddScore { get; set; }
        /// <summary>
        /// 关RankType：2；开1
        /// </summary>
        public int RankType { get; set; }
    }
}
