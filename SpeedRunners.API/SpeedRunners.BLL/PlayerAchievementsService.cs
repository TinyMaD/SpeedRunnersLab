using Microsoft.Extensions.Caching.Memory;
using SpeedRunners.Model.Steam;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeedRunners.BLL
{
    /// <summary>
    /// 玩家成就解锁状态缓存服务
    /// 避免每次访问个人主页都打 Steam API
    /// </summary>
    public class PlayerAchievementsService
    {
        private readonly IMemoryCache _cache;
        private readonly SteamBLL _steamBLL;
        private const string CacheKeyPrefix = "PlayerAchievements_";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

        public PlayerAchievementsService(IMemoryCache cache, SteamBLL steamBLL)
        {
            _cache = cache;
            _steamBLL = steamBLL;
        }

        /// <summary>
        /// 获取玩家成就解锁状态，命中缓存优先
        /// </summary>
        public async Task<List<MSteamAchievement>> GetPlayerAchievementsAsync(string steamId)
        {
            string cacheKey = CacheKeyPrefix + steamId;
            if (_cache.TryGetValue(cacheKey, out List<MSteamAchievement> cached))
            {
                return cached;
            }

            var data = await _steamBLL.GetPlayerAchievements(steamId);

            // 仅在拿到非空数据时缓存，失败/空结果允许下次重试
            if (data != null && data.Count > 0)
            {
                _cache.Set(cacheKey, data, _cacheDuration);
            }

            return data;
        }
    }
}
