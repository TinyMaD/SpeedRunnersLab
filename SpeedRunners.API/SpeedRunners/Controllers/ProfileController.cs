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
            => await BLL.GetProfileData(steamId);

        /// <summary>
        /// 获取每日天梯分历史记录（用于热度图）
        /// </summary>
        /// <param name="steamId">Steam ID</param>
        [HttpGet("{steamId}")]
        public List<MDailyScore> GetDailyScoreHistory(string steamId) 
            => BLL.GetDailyScoreHistory(steamId);

        /// <summary>
        /// 获取玩家成就
        /// </summary>
        /// <param name="steamId">Steam ID</param>
        [HttpGet("{steamId}")]
        public async Task<List<MAchievement>> GetAchievements(string steamId) 
            => await BLL.GetAchievements(steamId);
    }
}
