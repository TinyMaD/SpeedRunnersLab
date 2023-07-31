using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SpeedRunners.Model;
using SpeedRunners.Utils;
using System;

namespace SpeedRunners.Controllers
{
    public class BaseController<TBLL> : ControllerBase where TBLL : BaseBLL
    {
        protected TBLL BLL => GetBLL();

        private TBLL GetBLL()
        {
            TBLL bll = new Lazy<TBLL>(HttpContext.RequestServices.GetService<TBLL>()).Value;
            // 将注入的当前用户信息传入BLL中方便使用
            bll.CurrentUser = HttpContext.RequestServices.GetService<MUser>();
            return bll;
        }
    }
}
