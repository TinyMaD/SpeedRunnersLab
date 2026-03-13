using System;
using System.Collections.Generic;

namespace SpeedRunners.Model.Profile
{
    /// <summary>
    /// 个人主页数据
    /// </summary>
    public class MProfileData
    {
        public string PlatformID { get; set; }
        public string PersonaName { get; set; }
        public string AvatarS { get; set; }
        public string AvatarM { get; set; }
        public string AvatarL { get; set; }
        
        /// <summary>
        /// -1隐私权限, 0离线, 1在线, 2忙碌, 3离开, 4打盹, 5想交易, 6想玩游戏
        /// </summary>
        public int State { get; set; }
        
        /// <summary>
        /// 当前游戏ID，207140为SpeedRunners
        /// </summary>
        public string GameID { get; set; }
        
        /// <summary>
        /// 段位等级 0-9
        /// </summary>
        public int RankLevel { get; set; }
        
        /// <summary>
        /// 天梯分
        /// </summary>
        public decimal? RankScore { get; set; }
        
        /// <summary>
        /// 总游戏时长（小时）
        /// </summary>
        public decimal TotalPlaytime { get; set; }
        
        /// <summary>
        /// 最近两周游戏时长（小时）
        /// </summary>
        public decimal Past2WeeksPlaytime { get; set; }
        
        /// <summary>
        /// 最近两周新增天梯分
        /// </summary>
        public decimal Past2WeeksScore { get; set; }
        
        /// <summary>
        /// 游戏统计数据
        /// </summary>
        public List<MGameStat> Stats { get; set; }

        /// <summary>
        /// 是否为隐私主页（主页关闭时为true）
        /// </summary>
        public bool IsPrivate { get; set; }
    }

    /// <summary>
    /// 游戏统计项
    /// </summary>
    public class MGameStat
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
