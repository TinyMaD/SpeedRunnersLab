using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpeedRunners.Model;
using System.Net;

namespace SpeedRunners.Filter
{
    /// <summary>
    /// 全局异常处理
    /// </summary>
    public class GlobalExceptionsFilter : ExceptionFilterAttribute
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<GlobalExceptionsFilter> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="env">运行环境</param>
        public GlobalExceptionsFilter(IWebHostEnvironment env, ILogger<GlobalExceptionsFilter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (!context.ExceptionHandled && _env.IsProduction())
            {
                context.Result = new JsonResult(new MResponse
                {
                    Code = -1,
                    Message = "恭喜您发现了彩蛋(BUG),站长表示正在改..."
                });
                context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.ExceptionHandled = true;
                //采用log4net 进行错误日志记录
                _logger.LogError($"{context.Exception.Message}\r\n{context.Exception.StackTrace}");
            }
        }
    }
}
