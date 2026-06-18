using System.Collections.Generic;

namespace SpeedRunners.Model.Steam
{
    /// <summary>
    /// 玩家成就解锁状态查询结果状态
    /// </summary>
    public enum PlayerAchievementsStatus
    {
        /// <summary>
        /// 获取成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// Steam 资料不公开，无法获取
        /// </summary>
        ProfilePrivate = 1,

        /// <summary>
        /// 调用失败（网络错误、限流、解析失败等）
        /// </summary>
        Failed = 2
    }

    /// <summary>
    /// 玩家成就解锁状态查询结果
    /// </summary>
    public class MPlayerAchievementsResult
    {
        public PlayerAchievementsStatus Status { get; set; }

        /// <summary>
        /// 仅 Status 为 Success 时有值
        /// </summary>
        public List<MSteamAchievement> Achievements { get; set; }
    }
}
