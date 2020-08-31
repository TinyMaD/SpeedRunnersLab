using Microsoft.Extensions.DependencyInjection;
using SpeedRunners.BLL;
using SpeedRunners.Utils;
using System.Linq;

namespace SpeedRunners.Service
{
    public static class ServiceHelper
    {
        /// <summary>
        /// 批量注册所有BLL服务
        /// </summary>
        /// <param name="services"></param>
        public static void AddAllBLL(this IServiceCollection services)
        {
            // 获取BLL程序集下所有实现了BaseBLL的类
            typeof(SteamBLL).Assembly.GetTypes().ToList().ForEach(x =>
            {
                if (x.IsClass && !x.IsAbstract && typeof(BaseBLL).IsAssignableFrom(x))
                {
                    services.AddScoped(x);
                }
            });
        }
    }
}
