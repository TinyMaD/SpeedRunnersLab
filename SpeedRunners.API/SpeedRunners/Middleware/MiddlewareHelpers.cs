using Microsoft.AspNetCore.Builder;

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
    }
}
