using Microsoft.Extensions.Caching.Memory;
using SpeedRunners.Model.Steam;
using System;
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
        internal const string CacheKeyPrefix = "PlayerAchievements_";
        internal const string BackupCacheKeyPrefix = "PlayerAchievementsBackup_";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);
        // 备份保留更久，Steam API 偶发失败时回退到上一次成功的数据，而不是显示“全部未解锁”
        private readonly TimeSpan _backupCacheDuration = TimeSpan.FromDays(3);

        public PlayerAchievementsService(IMemoryCache cache, SteamBLL steamBLL)
        {
            _cache = cache;
            _steamBLL = steamBLL;
        }

        /// <summary>
        /// 获取玩家成就解锁状态，命中缓存优先；
        /// Steam 调用失败时回退到上一次成功的数据，没有备份才返回 Failed
        /// </summary>
        public async Task<MPlayerAchievementsResult> GetPlayerAchievementsAsync(string steamId)
        {
            string cacheKey = CacheKeyPrefix + steamId;
            if (_cache.TryGetValue(cacheKey, out MPlayerAchievementsResult cached))
            {
                return cached;
            }

            MPlayerAchievementsResult result = await _steamBLL.GetPlayerAchievements(steamId);

            if (result.Status == PlayerAchievementsStatus.Success)
            {
                _cache.Set(cacheKey, result, _cacheDuration);
                _cache.Set(BackupCacheKeyPrefix + steamId, result, _backupCacheDuration);
                return result;
            }

            if (result.Status == PlayerAchievementsStatus.ProfilePrivate)
            {
                // 私密资料是 Steam 明确返回的确定性结果，短期缓存避免反复打 Steam
                _cache.Set(cacheKey, result, _cacheDuration);
                return result;
            }

            // 调用失败：回退上一次成功的数据；不写新鲜缓存，下次请求继续尝试刷新
            if (_cache.TryGetValue(BackupCacheKeyPrefix + steamId, out MPlayerAchievementsResult backup))
            {
                return backup;
            }

            return result;
        }
    }
}
