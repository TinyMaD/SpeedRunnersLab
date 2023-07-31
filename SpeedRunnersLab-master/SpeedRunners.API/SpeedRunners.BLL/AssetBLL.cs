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
    }
}
