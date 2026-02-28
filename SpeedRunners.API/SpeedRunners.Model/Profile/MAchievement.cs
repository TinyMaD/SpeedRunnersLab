using System;

namespace SpeedRunners.Model.Profile
{
    /// <summary>
    /// 游戏成就
    /// </summary>
    public class MAchievement
    {
        public int Id { get; set; }
        
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
        /// 图标名称
        /// </summary>
        public string Icon { get; set; }
        
        /// <summary>
        /// 是否已解锁
        /// </summary>
        public bool Unlocked { get; set; }
        
        /// <summary>
        /// 解锁时间
        /// </summary>
        public DateTime? UnlockedAt { get; set; }
    }
}
