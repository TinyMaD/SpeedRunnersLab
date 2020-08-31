using SpeedRunners.Model;

namespace SpeedRunners.Utils
{
    public abstract class BaseBLL
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public MUser CurrentUser { get; set; }
    }
}
