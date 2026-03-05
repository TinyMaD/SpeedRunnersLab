using System;

namespace SpeedRunners.Model.Profile
{
    /// <summary>
    /// 游戏成就
    /// </summary>
    public class MAchievement
    {
        /// <summary>
        /// 成就API名称
        /// </summary>
        public string ApiName { get; set; }
        
        /// <summary>
        /// 成就显示名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 成就描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 已解锁成就图标URL
        /// </summary>
        public string IconUrl { get; set; }
        
        /// <summary>
        /// 未解锁成就图标URL（灰色版本）
        /// </summary>
        public string IconGrayUrl { get; set; }
        
        /// <summary>
        /// 是否已解锁
        /// </summary>
        public bool Unlocked { get; set; }
        
        /// <summary>
        /// 解锁时间
        /// </summary>
        public DateTime? UnlockedAt { get; set; }
        
        /// <summary>
        /// 是否为隐藏成就
        /// </summary>
        public bool Hidden { get; set; }
    }
}
