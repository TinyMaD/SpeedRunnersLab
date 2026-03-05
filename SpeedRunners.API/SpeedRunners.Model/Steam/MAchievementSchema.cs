using System;

namespace SpeedRunners.Model.Steam
{
    /// <summary>
    /// Steam游戏成就定义（从GetSchemaForGame API获取）
    /// </summary>
    public class MAchievementSchema
    {
        /// <summary>
        /// 成就API名称
        /// </summary>
        public string ApiName { get; set; }
        
        /// <summary>
        /// 成就显示名称
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// 成就描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 已解锁成就图标URL
        /// </summary>
        public string Icon { get; set; }
        
        /// <summary>
        /// 未解锁成就图标URL（灰色版本）
        /// </summary>
        public string IconGray { get; set; }
        
        /// <summary>
        /// 是否为隐藏成就
        /// </summary>
        public bool Hidden { get; set; }
    }
}
