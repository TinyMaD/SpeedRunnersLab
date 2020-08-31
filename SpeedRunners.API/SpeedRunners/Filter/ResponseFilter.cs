using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SpeedRunners.BLL;
using SpeedRunners.Model;
using System;

namespace SpeedRunners.Filter
{
    /// <summary>
    /// 统一处理响应参数
    /// </summary>
    public class ResponseFilter : ActionFilterAttribute
    {
        private readonly MUser _currentUser;
        private readonly UserBLL _loginBLL;
        public ResponseFilter(MUser currentUser, UserBLL loginBLL)
        {
            _currentUser = currentUser;
            _loginBLL = loginBLL;
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            // 统一出参格式
            MResponse response = new MResponse();
            if (context.Result is EmptyResult)
            {
                response = MResponse.Success();
            }
            else if (context.Result is ObjectResult obj)
            {
                if (obj.DeclaredType == typeof(MResponse))
                {
                    response = (MResponse)obj.Value;
                }
                else
                {
                    // 出参类型不为MResponse时，将出参赋值给MResponse.Data再返回
                    response = obj.Value.Success();
                }
            }

            // 统一刷新返回Token
            AddRefreshToken(context.HttpContext, response);

            context.Result = new ObjectResult(response);
            base.OnResultExecuting(context);
        }

        /// <summary>
        /// 刷新Token并添加到返回数据中
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        private void AddRefreshToken(HttpContext context, MResponse response)
        {
            Endpoint endpoint = context.GetEndpoint();
            // 检查请求接口标注的特性
            bool isPersona = endpoint?.Metadata.GetMetadata<PersonaAttribute>() != null;
            bool isUser = endpoint?.Metadata.GetMetadata<UserAttribute>() != null;
            // 获取用户发送的cookie
            string token = context.Request.Headers["srlab_token"];

            // 请求不需认证的接口，返回原Token
            if (!isPersona && !isUser)
            {
                if (!string.IsNullOrWhiteSpace(token))
                {
                    response.Token = token;
                }
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentUser?.PlatformID))
            {
                response.Token = null;
                return;
            }
            _currentUser.Token = token;
            response.Token = RefreshToken();
        }

        /// <summary>
        /// 刷新AccessToken
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        private string RefreshToken()
        {
            DateTime create = Convert.ToDateTime(_currentUser.Token.Split("&")[1]);
            int refresh = Convert.ToInt32(AppSettings.GetConfig("Refresh"));
            if (DateTime.Now - create > TimeSpan.FromMinutes(refresh))
            {
                // 过期则生成新的Token
                _currentUser.Token = CommonUtils.CreateToken();
                _loginBLL.UpdateAccessToken(_currentUser);
            }
            return _currentUser.Token;
        }
    }
}
