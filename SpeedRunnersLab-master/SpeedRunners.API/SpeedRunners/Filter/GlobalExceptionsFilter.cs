using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SpeedRunners.Model;
using System.IO;
using System.Net;
using System.Text;

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
                using var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8);
                string bodyStr = reader?.ReadToEnd();
                _logger.LogError($@"
【接口】：{context.HttpContext.Request.Path}
【参数】：{bodyStr}
【错误】{context.Exception.Message}
{context.Exception.StackTrace}");
            }
        }
    }
}
