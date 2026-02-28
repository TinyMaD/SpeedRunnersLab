using System;

namespace SpeedRunners.Model.Profile
{
    /// <summary>
    /// 每日天梯分记录（用于热度图）
    /// </summary>
    public class MDailyScore
    {
        /// <summary>
        /// 日期 (yyyy-MM-dd)
        /// </summary>
        public string Date { get; set; }
        
        /// <summary>
        /// 当日新增分数
        /// </summary>
        public int Score { get; set; }
    }
}
