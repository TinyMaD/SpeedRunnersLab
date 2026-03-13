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
        [HttpGet("{steamId}")]
        public async Task<MProfileData> GetData(string steamId)
        {
            // 获取当前登录用户的Steam ID（未登录时为null）
            string visitorId = BLL.CurrentUser?.PlatformID;
            return await BLL.GetProfileData(steamId, visitorId);
        }

        /// <summary>
        /// 获取每日天梯分历史记录（用于热度图）
        /// </summary>
        /// <param name="steamId">Steam ID</param>
        [HttpGet("{steamId}")]
        public List<MDailyScore> GetDailyScoreHistory(string steamId)
        {
            // 获取当前登录用户的Steam ID（未登录时为null）
            string visitorId = BLL.CurrentUser?.PlatformID;
            return BLL.GetDailyScoreHistory(steamId, visitorId);
        }

        /// <summary>
        /// 获取玩家成就
        /// </summary>
        /// <param name="steamId">Steam ID</param>
        [HttpGet("{steamId}")]
        public async Task<List<MAchievement>> GetAchievements(string steamId)
        {
            // 获取当前登录用户的Steam ID（未登录时为null）
            string visitorId = BLL.CurrentUser?.PlatformID;
            return await BLL.GetAchievements(steamId, visitorId);
        }
    }
}
