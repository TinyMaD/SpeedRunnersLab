using System.Collections.Generic;

namespace SpeedRunners.Model.Profile
{
    /// <summary>
    /// 个人主页成就接口返回
    /// </summary>
    public class MAchievementsResponse
    {
        public const string StatusOk = "ok";
        public const string StatusPrivate = "private";
        public const string StatusFailed = "failed";

        /// <summary>
        /// ok：数据正常；private：Steam 资料不公开；failed：Steam 数据获取失败，前端应提示重试
        /// </summary>
        public string Status { get; set; } = StatusOk;

        public List<MAchievement> Achievements { get; set; } = new List<MAchievement>();
    }
}
