using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
using SpeedRunners.Model.Profile;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeedRunners.Controllers
{
    /// <summary>
    /// 个人主页控制器
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProfileController : BaseController<ProfileBLL>
    {
        /// <summary>
        /// 获取个人主页数据
        /// </summary>
        /// <param name="steamId">Steam ID</param>
        [Persona]
        [HttpGet("{steamId}")]
        public async Task<MProfileData> GetData(string steamId)
        {
            string visitorId = BLL.CurrentUser?.PlatformID;
            return await BLL.GetProfileData(steamId, visitorId);
        }

        /// <summary>
        /// 获取每日天梯分历史记录（用于热度图）
        /// </summary>
        /// <param name="steamId">Steam ID</param>
        [Persona]
        [HttpGet("{steamId}")]
        public List<MDailyScore> GetDailyScoreHistory(string steamId)
        {
            string visitorId = BLL.CurrentUser?.PlatformID;
            return BLL.GetDailyScoreHistory(steamId, visitorId);
        }

        /// <summary>
        /// 获取玩家成就
        /// </summary>
        /// <param name="steamId">Steam ID</param>
        [Persona]
        [HttpGet("{steamId}")]
        public async Task<MAchievementsResponse> GetAchievements(string steamId)
        {
            string visitorId = BLL.CurrentUser?.PlatformID;
            return await BLL.GetAchievements(steamId, visitorId);
        }
    }
}
