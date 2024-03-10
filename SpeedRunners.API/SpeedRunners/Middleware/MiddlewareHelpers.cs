using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using SpeedRunners.Service;
using System.Collections.Generic;
using System.Globalization;

namespace SpeedRunners.Middleware
{
    public static class MiddlewareHelpers
    {
        /// <summary>
        /// 使用SRLabToken授权
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseSRLabTokenAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SRLabTokenAuthMidd>();
        }

        public static IApplicationBuilder UseHeaderRequestLocalization(this IApplicationBuilder app)
        {
            return app.UseRequestLocalization(options =>
            {
                // 配置可支持的语言
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("zh")
                };
                options.DefaultRequestCulture = new RequestCulture("en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new LocaleHeaderRequestCultureProvider()
                };
            });
        }
    }
}
