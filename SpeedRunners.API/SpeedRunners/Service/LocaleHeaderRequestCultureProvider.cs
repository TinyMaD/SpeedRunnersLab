using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using System.Threading.Tasks;

namespace SpeedRunners.Service
{
    public class LocaleHeaderRequestCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            string locale = httpContext.Request.Headers["locale"].ToString();

            return Task.FromResult(new ProviderCultureResult(locale == "zh" ? "zh" : "en"));
        }
    }
}
