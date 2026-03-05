using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using SpeedRunners.Model.Steam;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeedRunners.BLL
{
    /// <summary>
    /// 成就定义缓存服务
    /// 从Steam获取游戏的成就定义并缓存，避免频繁调用Steam API
    /// </summary>
    public class AchievementSchemaService
    {
        private readonly IMemoryCache _cache;
        private readonly SteamBLL _steamBLL;
        private const string CacheKey = "SpeedRunners_AchievementSchema";
        // 缓存24小时，成就定义不会频繁变化
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(24);
        private static readonly uint AppId = 207140;

        public AchievementSchemaService(IMemoryCache cache, SteamBLL steamBLL)
        {
            _cache = cache;
            _steamBLL = steamBLL;
        }

        /// <summary>
        /// 获取SpeedRunners游戏的成就定义列表
        /// </summary>
        public async Task<List<MAchievementSchema>> GetAchievementSchemaAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<MAchievementSchema> cachedSchema))
            {
                return cachedSchema;
            }

            var schema = await FetchAchievementSchemaFromSteam();
            
            if (schema != null && schema.Any())
            {
                _cache.Set(CacheKey, schema, _cacheDuration);
            }

            return schema;
        }

        /// <summary>
        /// 从Steam API获取成就定义
        /// </summary>
        private async Task<List<MAchievementSchema>> FetchAchievementSchemaFromSteam()
        {
            string apiKey = AppSettings.GetConfig("ApiKey");
            string url = $"https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key={apiKey}&appid={AppId}";
            
            try
            {
                string response = await HttpHelper.HttpGet(url);
                if (string.IsNullOrWhiteSpace(response))
                {
                    return new List<MAchievementSchema>();
                }

                JObject obj = JObject.Parse(response);
                var game = obj["game"];
                var availableGameStats = game?["availableGameStats"];
                var achievements = availableGameStats?["achievements"];

                if (achievements == null)
                {
                    return new List<MAchievementSchema>();
                }

                var result = new List<MAchievementSchema>();
                foreach (var ach in achievements)
                {
                    var schema = new MAchievementSchema
                    {
                        ApiName = ach["name"]?.ToString(),
                        DisplayName = ach["displayName"]?.ToString(),
                        Description = ach["description"]?.ToString() ?? "",
                        Icon = ach["icon"]?.ToString(),
                        IconGray = ach["icongray"]?.ToString(),
                        Hidden = ach["hidden"]?.ToString() == "1"
                    };
                    result.Add(schema);
                }

                return result;
            }
            catch (Exception)
            {
                return new List<MAchievementSchema>();
            }
        }

        /// <summary>
        /// 刷新缓存
        /// </summary>
        public async Task RefreshCacheAsync()
        {
            _cache.Remove(CacheKey);
            await GetAchievementSchemaAsync();
        }
    }
}
