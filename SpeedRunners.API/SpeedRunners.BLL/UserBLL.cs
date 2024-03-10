using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.Rank;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SpeedRunners.BLL
{
    public class UserBLL : BLLHelper<UserDAL>
    {
        private static RankBLL _rankBLL;
        private readonly ILogger<UserBLL> _logger;
        public UserBLL(RankBLL rankBLL, ILogger<UserBLL> logger)
        {
            _rankBLL = rankBLL;
            _logger = logger;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public MRankInfo GetInfo()
        {
            return _rankBLL.GetRankList(new[] { CurrentUser.PlatformID }).FirstOrDefault();
        }

        public async Task<MResponse> Login(string query, string browser)
        {
            string openIDUrl = "https://steamcommunity.com/openid/login";
            query = Regex.Replace(query, "(?<=openid.mode=).+?(?=\\&)", "check_authentication", RegexOptions.IgnoreCase).Trim('?');
            string result = string.Empty;
            try
            {
                result = await HttpHelper.HttpPost(openIDUrl, query);
            }
            catch (Exception)
            {
                // 超时登录失败
                return MResponse.Fail(Localizer["login_timeout"], -555);
            }
            if (result.ToLower().Contains("is_valid:true"))
            {
                string steamID = Regex.Match(HttpUtility.UrlDecode(query), "(?<=openid/id/)\\d+", RegexOptions.IgnoreCase).Value;
                string newToken = CommonUtils.CreateToken();
                // 保存登录凭证
                BeginDb(DAL =>
                {
                    DAL.AddAccessToken(new MUser
                    {
                        PlatformID = steamID,
                        Browser = browser,
                        Token = newToken
                    });
                });
                MResponse response = MResponse.Success();
                response.Token = newToken;
                return response;
            }
            return MResponse.Fail(Localizer["login_fail"]);
        }

        public MAccessToken GetUserByToken(string token)
        {
            return BeginDb(DAL =>
            {
                MAccessToken user = DAL.GetUserByToken(token);
                if (token.Equals(user?.ExToken))
                {
                    DateTime create = Convert.ToDateTime(user.Token.Split("&")[1]);
                    int expire = Convert.ToInt32(AppSettings.GetConfig("Refresh")) - 1;
                    if (DateTime.Now - create > TimeSpan.FromMinutes(expire))
                    {
                        return null;
                    }
                }
                return user;
            });
        }

        public void UpdateAccessToken(MAccessToken user)
        {
            BeginDb(DAL =>
            {
                DAL.UpdateAccessToken(user);
            });
        }

        public MResponse DeleteAccessToken(int tokenID)
        {
            return BeginDb(DAL =>
            {
                MUser deleteUser = DAL.GetUserByTokenID(tokenID);
                if (deleteUser == null)
                {
                    return MResponse.Fail(Localizer["logout_already"]);
                }
                if (deleteUser.PlatformID != CurrentUser.PlatformID)
                {
                    return MResponse.Fail(Localizer["permission_error"]);
                }
                if (deleteUser.LoginDate > CurrentUser.LoginDate)
                {
                    return MResponse.Fail(Localizer["low_permission"], -401);
                }
                DAL.DeleteAccessToken(deleteUser);
                return MResponse.Success();
            });
        }

        public void DeleteAccessToken()
        {
            BeginDb(DAL =>
            {
                DAL.DeleteAccessToken(CurrentUser.Token);
            });
            CurrentUser.PlatformID = null;
        }
    }
}
