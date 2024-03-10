using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using SpeedRunners.Model;

namespace SpeedRunners.Utils
{
    public abstract class BaseBLL
    {
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public MUser CurrentUser { get; set; }
        public HttpContext HttpContext { get; set; }
        public IStringLocalizer Localizer { get; set; }
    }
}
