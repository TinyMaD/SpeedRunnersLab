﻿using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
using SpeedRunners.Model;
using SpeedRunners.Model.Rank;
using SpeedRunners.Model.User;
using System.Threading.Tasks;

namespace SpeedRunners.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : BaseController<UserBLL>
    {
        [User]
        [HttpGet]
        public MRankInfo GetInfo() => BLL.GetInfo();

        [User]
        [HttpGet]
        public MPrivacySettings GetPrivacySettings() => BLL.GetPrivacySettings();

        [User]
        [HttpPost]
        public void SetState([FromBody] dynamic obj) => BLL.SetStateOrRankType("State", (int)obj.value.Value);

        [User]
        [HttpPost]
        public void SetRankType([FromBody] dynamic obj) => BLL.SetStateOrRankType("RankType", (int)obj.value.Value);

        [User]
        [HttpPost]
        public void SetShowWeekPlayTime([FromBody] dynamic obj) => BLL.SetPrivacySettings("ShowWeekPlayTime", (int)obj.value.Value);

        [User]
        [HttpPost]
        public void SetRequestRankData([FromBody] dynamic obj) => BLL.SetPrivacySettings("RequestRankData", (int)obj.value.Value);

        [User]
        [HttpPost]
        public void SetShowAddScore([FromBody] dynamic obj) => BLL.SetPrivacySettings("ShowAddScore", (int)obj.value.Value);

        [HttpPost]
        public async Task<MResponse> Login([FromBody] dynamic data)
        {
            string browser = HttpContext.Request?.Headers?["User-Agent"] ?? "未知设备";
            return await BLL.Login(data.query.Value, browser);
        }

        [User]
        [HttpGet("{tokenID}")]
        public MResponse LogoutOther(int tokenID) => BLL.DeleteAccessToken(tokenID);

        [User]
        [HttpGet]
        public void LogoutLocal() => BLL.DeleteAccessToken();
    }
}
