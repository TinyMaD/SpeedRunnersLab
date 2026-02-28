using System;

namespace SpeedRunners.Model.Steam
{
    /// <summary>
    /// Steam玩家成就
    /// </summary>
    public class MSteamAchievement
    {
        public string ApiName { get; set; }
        public int Achieved { get; set; }
        public DateTime? UnlockTime { get; set; }
    }
}
