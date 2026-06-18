using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using SpeedRunners.BLL;
using SpeedRunners.Model.Steam;

namespace SpeedRunners.Tests
{
    public class PlayerAchievementsServiceTests
    {
        private const string SteamId = "76561198000000100";

        private sealed class FakeSteamBLL : SteamBLL
        {
            private readonly Queue<MPlayerAchievementsResult> _results = new();

            public FakeSteamBLL() : base(NullLogger<SteamBLL>.Instance) { }

            public int CallCount { get; private set; }

            public void Enqueue(MPlayerAchievementsResult result) => _results.Enqueue(result);

            public override Task<MPlayerAchievementsResult> GetPlayerAchievements(string steamID)
            {
                CallCount++;
                return Task.FromResult(_results.Dequeue());
            }
        }

        private static MPlayerAchievementsResult Success() => new()
        {
            Status = PlayerAchievementsStatus.Success,
            Achievements = new List<MSteamAchievement> { new() { ApiName = "ACH_1", Achieved = 1 } }
        };

        private static MPlayerAchievementsResult Failed() => new() { Status = PlayerAchievementsStatus.Failed };

        private static MPlayerAchievementsResult Private() => new() { Status = PlayerAchievementsStatus.ProfilePrivate };

        [Fact]
        public async Task SecondCall_HitsCache_WithoutCallingSteamAgain()
        {
            var steam = new FakeSteamBLL();
            steam.Enqueue(Success());
            var service = new PlayerAchievementsService(new MemoryCache(new MemoryCacheOptions()), steam);

            var first = await service.GetPlayerAchievementsAsync(SteamId);
            var second = await service.GetPlayerAchievementsAsync(SteamId);

            Assert.Equal(1, steam.CallCount);
            Assert.Equal(PlayerAchievementsStatus.Success, second.Status);
            Assert.Same(first.Achievements, second.Achievements);
        }

        [Fact]
        public async Task FallsBackToLastSuccess_WhenSteamFails()
        {
            var steam = new FakeSteamBLL();
            steam.Enqueue(Success());
            steam.Enqueue(Failed());
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = new PlayerAchievementsService(cache, steam);

            var first = await service.GetPlayerAchievementsAsync(SteamId);
            // 模拟新鲜缓存过期（备份缓存仍在）
            cache.Remove(PlayerAchievementsService.CacheKeyPrefix + SteamId);
            var second = await service.GetPlayerAchievementsAsync(SteamId);

            Assert.Equal(2, steam.CallCount);
            Assert.Equal(PlayerAchievementsStatus.Success, second.Status);
            Assert.Same(first.Achievements, second.Achievements);
        }

        [Fact]
        public async Task ReturnsFailed_WhenSteamFailsAndNoBackup()
        {
            var steam = new FakeSteamBLL();
            steam.Enqueue(Failed());
            var service = new PlayerAchievementsService(new MemoryCache(new MemoryCacheOptions()), steam);

            var result = await service.GetPlayerAchievementsAsync(SteamId);

            Assert.Equal(PlayerAchievementsStatus.Failed, result.Status);
        }

        [Fact]
        public async Task FailedResult_IsNotCached_SoNextCallRetries()
        {
            var steam = new FakeSteamBLL();
            steam.Enqueue(Failed());
            steam.Enqueue(Success());
            var service = new PlayerAchievementsService(new MemoryCache(new MemoryCacheOptions()), steam);

            var first = await service.GetPlayerAchievementsAsync(SteamId);
            var second = await service.GetPlayerAchievementsAsync(SteamId);

            Assert.Equal(2, steam.CallCount);
            Assert.Equal(PlayerAchievementsStatus.Failed, first.Status);
            Assert.Equal(PlayerAchievementsStatus.Success, second.Status);
        }

        [Fact]
        public async Task ProfilePrivateResult_IsCached()
        {
            var steam = new FakeSteamBLL();
            steam.Enqueue(Private());
            var service = new PlayerAchievementsService(new MemoryCache(new MemoryCacheOptions()), steam);

            var first = await service.GetPlayerAchievementsAsync(SteamId);
            var second = await service.GetPlayerAchievementsAsync(SteamId);

            Assert.Equal(1, steam.CallCount);
            Assert.Equal(PlayerAchievementsStatus.ProfilePrivate, first.Status);
            Assert.Equal(PlayerAchievementsStatus.ProfilePrivate, second.Status);
        }
    }
}
