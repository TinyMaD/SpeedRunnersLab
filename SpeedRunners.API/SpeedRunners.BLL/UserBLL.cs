using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.Rank;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
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

        public MPrivacySettings GetPrivacySettings()
        {
            return BeginDb(DAL =>
            {
                MPrivacySettings settings = DAL.GetPrivacySettings(CurrentUser.PlatformID);
                return settings;
            });
        }

        public void SetStateOrRankType(string colName, int value)
        {
            if (colName.Equals("State", StringComparison.OrdinalIgnoreCase))
            {
                if (value != -1 && value != 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "State must be -1 or 0");
            }
            else if (colName.Equals("RankType", StringComparison.OrdinalIgnoreCase))
            {
                if (value != 1 && value != 2)
                    throw new ArgumentOutOfRangeException(nameof(value), "RankType must be 1 or 2");
            }
            BeginDb(DAL =>
            {
                DAL.SetStateOrRankType(CurrentUser.PlatformID, colName, value);
            });
        }

        public void SetPrivacySettings(string colName, int value)
        {
            if (value != 0 && value != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Privacy switch value must be 0 or 1");
            }
            BeginDb(DAL =>
            {
                DAL.SetPrivacySettings(CurrentUser.PlatformID, colName, value);
            });
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public MRankInfo GetInfo()
        {
            var info = _rankBLL.GetAllRankList(new[] { CurrentUser.PlatformID }).FirstOrDefault();
            if (info != null)
            {
                info.IsAdmin = AdminHelper.IsAdmin(CurrentUser.PlatformID);
            }
            return info;
        }

        public async Task<MResponse> Login(string query, string browser, string oldToken)
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
                string responseToken = null;
                // 保存登录凭证
                BeginDb(DAL =>
                {
                    MAccessToken oldSession = string.IsNullOrWhiteSpace(oldToken) ? null : DAL.GetUserByToken(oldToken);
                    if (oldSession?.PlatformID == steamID)
                    {
                        DAL.RefreshSession(new MAccessToken
                        {
                            TokenID = oldSession.TokenID,
                            PlatformID = steamID,
                            Browser = browser
                        });
                        responseToken = oldSession.Token;
                        return;
                    }

                    responseToken = CommonUtils.CreateToken();
                    DAL.AddAccessToken(new MUser
                    {
                        PlatformID = steamID,
                        Browser = browser,
                        Token = responseToken
                    });
                });
                MResponse response = MResponse.Success();
                response.Token = responseToken;
                return response;
            }
            return MResponse.Fail(Localizer["login_fail"]);
        }

        public MAccessToken GetUserByToken(string token)
        {
            return BeginDb(DAL =>
            {
                return DAL.GetUserByToken(token);
            });
        }

        public List<MDevice> GetDevices()
        {
            return BeginDb(DAL =>
            {
                return DAL.GetDevicesByPlatformID(CurrentUser.PlatformID)
                    .Select(x =>
                    {
                        bool isCurrent = x.TokenID == CurrentUser.TokenID;
                        return new MDevice
                        {
                            TokenID = x.TokenID,
                            DeviceName = GetDeviceName(x.Browser),
                            LoginDate = x.LoginDate,
                            LastActiveTime = x.LastActiveTime ?? x.LoginDate,
                            IsCurrent = isCurrent,
                            CanLogout = !isCurrent && x.LoginDate <= CurrentUser.LoginDate
                        };
                    })
                    .ToList();
            });
        }

        public void TouchLastActive(int tokenID)
        {
            BeginDb(DAL =>
            {
                DAL.TouchLastActive(tokenID);
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

        private string GetDeviceName(string browser)
        {
            if (string.IsNullOrWhiteSpace(browser))
            {
                return "Unknown Device";
            }

            string browserName = browser;
            if (browser.Contains("Edg/", StringComparison.OrdinalIgnoreCase))
            {
                browserName = "Microsoft Edge";
            }
            else if (browser.Contains("Chrome/", StringComparison.OrdinalIgnoreCase))
            {
                browserName = "Chrome";
            }
            else if (browser.Contains("Firefox/", StringComparison.OrdinalIgnoreCase))
            {
                browserName = "Firefox";
            }
            else if (browser.Contains("Safari/", StringComparison.OrdinalIgnoreCase))
            {
                browserName = "Safari";
            }

            string osName = "";
            if (browser.Contains("Windows", StringComparison.OrdinalIgnoreCase))
            {
                osName = "Windows";
            }
            else if (browser.Contains("Mac OS", StringComparison.OrdinalIgnoreCase))
            {
                osName = "macOS";
            }
            else if (browser.Contains("Android", StringComparison.OrdinalIgnoreCase))
            {
                osName = "Android";
            }
            else if (browser.Contains("iPhone", StringComparison.OrdinalIgnoreCase) || browser.Contains("iPad", StringComparison.OrdinalIgnoreCase))
            {
                osName = "iOS";
            }
            else if (browser.Contains("Linux", StringComparison.OrdinalIgnoreCase))
            {
                osName = "Linux";
            }

            return string.IsNullOrWhiteSpace(osName) ? browserName : $"{browserName} · {osName}";
        }
    }
}
