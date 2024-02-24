using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Qiniu.Storage;
using Qiniu.Util;
using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.Asset;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SpeedRunners.BLL
{
    public class AssetBLL : BLLHelper<AssetDAL>
    {
        private static readonly string _accessKey = AppSettings.GetConfig("AccessKey");
        private static readonly string _secretKey = AppSettings.GetConfig("SecretKey");
        private static readonly string _afdianUserID = AppSettings.GetConfig("AfdianUserID");
        private static readonly string _afdianToken = AppSettings.GetConfig("AfdianToken");
        public string[] CreateUploadToken()
        {
            Mac mac = new Mac(_accessKey, _secretKey);
            PutPolicy imgPolicy = new PutPolicy
            {
                Scope = "sr-img"
            };
            PutPolicy modPolicy = new PutPolicy
            {
                Scope = "sr-mod"
            };
            string imgToken = Auth.CreateUploadToken(mac, imgPolicy.ToJsonString());
            string fileToken = Auth.CreateUploadToken(mac, modPolicy.ToJsonString());
            return new[] { imgToken, fileToken };
        }
        public string CreateDownloadUrl(string key, string domain = "https://cdn-mod.speedrunners.cn")
        {
            Mac mac = new Mac(_accessKey, _secretKey);
            string privateUrl = DownloadManager.CreatePrivateUrl(mac, domain, key, 10 * 60);
            BeginDb(DAL =>
            {
                DAL.UpdateDownloadNum(key);
            });
            return privateUrl;
        }

        public MPageResult<MMod> GetModList(MModPageParam param)
        {
            MPageResult<MMod> result = new MPageResult<MMod>();
            BeginDb(DAL =>
            {
                string currentUserID = CurrentUser?.PlatformID ?? string.Empty;
                result = DAL.GetModList(param, currentUserID);
                foreach (MMod item in result.List)
                {
                    item.ImgUrl = "https://cdn-img.speedrunners.cn/" + item.ImgUrl;
                }
            });
            return result;
        }

        public void AddMod(MMod param)
        {
            param.AuthorID = CurrentUser.PlatformID;
            BeginDb(DAL =>
            {
                DAL.AddMod(param);
            });
        }

        public void OperateModStar(int modID, bool Star)
        {
            BeginDb(DAL =>
            {
                if (Star)
                {
                    DAL.AddModStar(modID, CurrentUser.PlatformID);
                }
                else
                {
                    DAL.DeleteModStar(modID, CurrentUser.PlatformID);
                }
            });
        }

        public async Task<MResponse> GetAfdianSponsorAsync()
        {
            string parameters = "{\"per_page\":100,\"page\":1}";
            long timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

            string signature = SignatureHelper.GenerateSignature(_afdianToken, _afdianUserID, parameters, timestamp);

            JObject jsonObject = new JObject
            {
                { "user_id", _afdianUserID },
                { "params", parameters },
                { "ts", timestamp },
                { "sign", signature }
            };

            string jsonString = jsonObject.ToString();
            using HttpClient httpClient = new HttpClient();

            HttpContent content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            string url = "https://afdian.net/api/open/query-sponsor";
            HttpResponseMessage response = await httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                return MResponse.Fail("afdian接口请求失败");
            }
            string responseContent = await response.Content.ReadAsStringAsync();

            JObject obj = JObject.Parse(responseContent);
            if (obj["ec"].ToString() == "200")
            {
                return obj["data"].Success();
            }
            else
            {
                return MResponse.Fail(obj["em"].ToString());
            }
        }
    }
}
