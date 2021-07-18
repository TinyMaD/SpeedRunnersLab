using Qiniu.Storage;
using Qiniu.Util;
using SpeedRunners.DAL;
using SpeedRunners.Model;
using SpeedRunners.Model.Asset;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.BLL
{
    public class AssetBLL : BLLHelper<AssetDAL>
    {
        private static readonly string _accessKey = AppSettings.GetConfig("AccessKey");
        private static readonly string _secretKey = AppSettings.GetConfig("SecretKey");
        public string CreateUploadToken()
        {
            Mac mac = new Mac(_accessKey, _secretKey);
            PutPolicy putPolicy = new PutPolicy
            {
                Scope = "sr-mod"
            };
            string token = Auth.CreateUploadToken(mac, putPolicy.ToJsonString());
            return token;
        }
        public string CreateDownloadUrl(string key, string domain = "http://cdn-mod.speedrunners.cn")
        {
            Mac mac = new Mac(_accessKey, _secretKey);
            string privateUrl = DownloadManager.CreatePrivateUrl(mac, domain, key, 10 * 60);
            return privateUrl;
        }

        public MPageResult<MMod> GetModList(MModPageParam param)
        {
            MPageResult<MMod> result = new MPageResult<MMod>();
            BeginDb(DAL =>
            {
                result = DAL.GetModList(param);
            });
            return result;
        }
    }
}
