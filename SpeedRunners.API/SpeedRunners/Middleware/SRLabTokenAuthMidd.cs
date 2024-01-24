using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SpeedRunners.BLL;
using SpeedRunners.Filter;
using SpeedRunners.Model;
using SpeedRunners.Model.User;
using System.Threading.Tasks;

namespace SpeedRunners.Middleware
{
    /// <summary>
    /// 自定义授权中间件
    /// </summary>
    public class SRLabTokenAuthMidd
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionsFilter> _logger;
        public SRLabTokenAuthMidd(RequestDelegate next, ILogger<GlobalExceptionsFilter> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            // 用户认证
            AuthResult authResult = AuthUser(context);
            switch (authResult)
            {
                case AuthResult.DontNeed:
                case AuthResult.AuthSuccess:
                    await _next(context);
                    break;
                case AuthResult.AuthFail:
                    // 未登录，不能访问目标接口
                    context.Request.ContentType = "application/json;charset=utf-8";
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(MResponse.Fail("未登录"), new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
                    break;
            }
        }

        /// <summary>
        /// 用户认证
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public AuthResult AuthUser(HttpContext context)
        {
            Endpoint endpoint = context.GetEndpoint();
            // 检查请求接口标注的特性
            bool isPersona = endpoint?.Metadata.GetMetadata<PersonaAttribute>() != null;
            bool isUser = endpoint?.Metadata.GetMetadata<UserAttribute>() != null;
            string token = context.Request.Headers["srlab-token"];
            if (!isPersona && !isUser)
            {
                return AuthResult.DontNeed;
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                if (isUser)
                {
                    return AuthResult.AuthFail;
                }
                else
                {
                    return AuthResult.DontNeed;
                }
            }

            // 获取当前用户信息
            UserBLL loginBLL = context.RequestServices.GetService<UserBLL>();
            MAccessToken user = loginBLL.GetUserByToken(token);
            if (string.IsNullOrWhiteSpace(user?.PlatformID))
            {
                if (isUser)
                {
                    return AuthResult.AuthFail;
                }
                else
                {
                    return AuthResult.DontNeed;
                }
            }
            // 给当前用户信息Service赋值
            MUser currentUser = context.RequestServices.GetService<MUser>();
            string browser = context.Request?.Headers?["User-Agent"];
            currentUser.Browser = browser ?? "未知设备";
            currentUser.TokenID = user.TokenID;
            currentUser.Token = user.Token;
            currentUser.PlatformID = user.PlatformID;
            currentUser.LoginDate = user.LoginDate;
            return AuthResult.AuthSuccess;
        }
    }

    /// <summary>
    /// 认证结果
    /// </summary>
    public enum AuthResult
    {
        /// <summary>
        /// 不需要认证
        /// </summary>
        DontNeed = 0,
        /// <summary>
        /// 认证失败
        /// </summary>
        AuthFail = 1,
        /// <summary>
        /// 认证成功
        /// </summary>
        AuthSuccess = 2
    }
}
