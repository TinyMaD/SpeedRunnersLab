using Dapper;
using FluentScheduler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedRunners.Scheduler
{
    internal class Task
    {
        private static readonly string STOP_UPDATE = ConfigurationManager.AppSettings["StopUpdateName"];
        private static readonly string MINUTES_NUM = ConfigurationManager.AppSettings["MinutesNum"];
        private static readonly string SECOND_NUM = ConfigurationManager.AppSettings["SecondNum"];
        private static readonly string RUN_UPDATESTEAMINFO = ConfigurationManager.AppSettings["RunUpdateSteamInfo"];
        private static readonly string ApiKey = ConfigurationManager.AppSettings["ApiKey"];
        private static readonly int SteamActiveWindowMinutes = GetIntAppSetting("SteamActiveWindowMinutes", 30);
        private static readonly int SteamSummaryActiveTtlMinutes = GetIntAppSetting("SteamSummaryActiveTtlMinutes", 5);
        private static readonly int SteamPlayTimeActiveTtlMinutes = GetIntAppSetting("SteamPlayTimeActiveTtlMinutes", 60);
        private static readonly int SteamDailyTtlHours = GetIntAppSetting("SteamDailyTtlHours", 24);
        private static readonly int SteamSummaryBatchSize = GetIntAppSetting("SteamSummaryBatchSize", 100);
        private static readonly int SteamPlayTimeBatchSize = GetIntAppSetting("SteamPlayTimeBatchSize", 5);
        private static readonly int SteamUpdateLoopDelaySeconds = GetIntAppSetting("SteamUpdateLoopDelaySeconds", 30);
        private static readonly int SteamFailureBackoffMinutes = GetIntAppSetting("SteamFailureBackoffMinutes", 15);
        private static readonly int SteamPlayTimeRequestDelayMilliseconds = GetIntAppSetting("SteamPlayTimeRequestDelayMilliseconds", 500);
        private static readonly LogHelper _log = LogHelper.GetCurrentClassLogHelper();
        private readonly Dictionary<string, DateTime> _steamFailureUntil = new Dictionary<string, DateTime>();
        public void Execute()
        {
            try
            {
                //await UpdateSteamInfo();
                //await UpdatePlayTime();
                //await UpdateScore();

                Registry timer = new Registry();
                timer.NonReentrantAsDefault();
                _ = timer.ScheduleEx(_log, () => UpdateScore()).ToRunNow().AndEvery(10)
                    .Minutes();

                _ = timer.ScheduleEx(_log, () => UpdateOldScore()).ToRunEvery(1).Days()
                    .At(18, 0);

                _ = timer.ScheduleEx(_log, () => InsertRankLog()).ToRunEvery(1).Days()
                    .At(17, 30);

                _ = timer.ScheduleEx(_log, () => DeleteExpiredAccessTokens()).ToRunEvery(1).Days()
                    .At(3, 0);

                JobManager.Initialize(timer);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
        private void InsertRankLog()
        {
            using (IDbConnection conn = DbHelper.GetConnection())
            {
                int rows = conn.Execute($@"
                INSERT INTO RankLog(PlatformID, RankScore)
                SELECT  PlatformID,
                        RankScore
                FROM RankInfo
                WHERE RankScore > 0 AND RankScore <> OldRankScore");
                Console.WriteLine($"成功添加{rows}条【天梯分日志】({DateTime.Now})");
            }
        }

        private void DeleteExpiredAccessTokens()
        {
            int days = 180;
            if (int.TryParse(ConfigurationManager.AppSettings["TokenExpireDays"], out int configDays) && configDays > 0)
            {
                days = configDays;
            }
            using (IDbConnection conn = DbHelper.GetConnection())
            {
                int rows = conn.Execute($@"
                DELETE FROM AccessToken
                WHERE COALESCE(LastActiveTime, LoginDate) < DATE_SUB(NOW(), INTERVAL ?{nameof(days)} DAY)", new { days });
                Console.WriteLine($"成功清理{rows}条【过期登录凭证】({DateTime.Now})");
            }
        }

        private async System.Threading.Tasks.Task UpdatePlayTime(List<string> platformIDs, HttpClient httpClient)
        {
            if (platformIDs == null || !platformIDs.Any())
            {
                return;
            }

            Console.WriteLine($"更新【{platformIDs.Count}】个Steam游戏时间({DateTime.Now})");
            List<RankInfoModel> rankParamList = new List<RankInfoModel>();
            List<string> failedIDs = new List<string>();
            foreach (string platformID in platformIDs)
            {
                RankInfoModel playTime = await FetchPlayTime(platformID, httpClient);
                if (playTime == null)
                {
                    failedIDs.Add(platformID);
                    continue;
                }
                rankParamList.Add(playTime);
            }

            if (rankParamList.Any())
            {
                using (IDbConnection conn = DbHelper.GetConnection())
                {
                    conn.Execute($@"
                        UPDATE RankInfo
                        SET WeekPlayTime = ?{nameof(RankInfoModel.WeekPlayTime)},
                            PlayTime = ?{nameof(RankInfoModel.PlayTime)},
                            PlayTimeModifyTime = NOW()
                        WHERE PlatformID = ?{nameof(RankInfoModel.PlatformID)}",
                        rankParamList);
                }
            }

            MarkSteamFailures(failedIDs);
            Console.WriteLine($"Steam游戏时间更新【{rankParamList.Count}/{platformIDs.Count}】({DateTime.Now})");
        }

        private async Task<RankInfoModel> FetchPlayTime(string platformID, HttpClient httpClient)
        {
            try
            {
                string recentUrl = "https://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v1/?key=" + ApiKey + $"&steamid={platformID}";
                string recentResponse = await httpClient.HttpGetAsync(recentUrl);
                int? weekTime = TryReadWeekPlayTime(recentResponse);
                await System.Threading.Tasks.Task.Delay(SteamPlayTimeRequestDelayMilliseconds);

                string ownedUrl = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key=" + ApiKey + $"&steamid={platformID}";
                string ownedResponse = await httpClient.HttpGetAsync(ownedUrl);
                int? totalTime = TryReadTotalPlayTime(ownedResponse);

                if (!weekTime.HasValue || !totalTime.HasValue)
                {
                    return null;
                }

                return new RankInfoModel
                {
                    PlatformID = platformID,
                    PlayTime = totalTime.Value,
                    WeekPlayTime = weekTime.Value
                };
            }
            catch (Exception ex)
            {
                _log.Log($"更新Steam游戏时间失败【{platformID}】：{ex}");
                return null;
            }
        }

        private static int? TryReadWeekPlayTime(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }

            JObject obj = JObject.Parse(response);
            JToken jt = obj["response"];
            if (jt == null || jt["total_count"] == null)
            {
                return null;
            }

            if ((int)jt["total_count"] <= 0)
            {
                return 0;
            }

            JArray games = jt["games"] as JArray;
            if (games == null)
            {
                return 0;
            }

            foreach (JToken game in games)
            {
                if (game["appid"]?.ToString() == "207140")
                {
                    return game["playtime_2weeks"] == null ? 0 : (int)game["playtime_2weeks"];
                }
            }

            return 0;
        }

        private static int? TryReadTotalPlayTime(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }

            JObject obj = JObject.Parse(response);
            JToken jt = obj["response"];
            if (jt == null || jt["game_count"] == null)
            {
                return null;
            }

            if ((int)jt["game_count"] <= 0)
            {
                return 0;
            }

            JArray games = jt["games"] as JArray;
            if (games == null)
            {
                return 0;
            }

            foreach (JToken game in games)
            {
                if (game["appid"]?.ToString() == "207140")
                {
                    return game["playtime_forever"] == null ? 0 : (int)game["playtime_forever"];
                }
            }

            return 0;
        }
        private void UpdateOldScore()
        {
            using (IDbConnection conn = DbHelper.GetConnection())
            {
                conn.Execute("UPDATE RankInfo SET OldRankScore = RankScore WHERE RankType <> 0");
                Console.WriteLine($"成功更新【每日天梯分】信息({DateTime.Now})");
            }
        }

        private async System.Threading.Tasks.Task UpdateScore()
        {
            using (IDbConnection conn = DbHelper.GetConnection())
            {
                IEnumerable<string> rankID = conn.Query<string>($@"SELECT info.RankID 
                                                                    FROM RankInfo info
                                                                    LEFT JOIN PrivacySettings ps ON ps.PlatformID = info.PlatformID
                                                                    WHERE RankType <> 0 AND (ISNULL(ps.RequestRankData) OR ps.RequestRankData = 1) ");
                List<RankInfoModel> newInfoList = await RankInfoListAsync(rankID);
                StringBuilder sql = new StringBuilder();
                foreach (RankInfoModel item in newInfoList)
                {
                    sql.AppendLine($" update RankInfo set RankScore={item.RankScore},RankCount={item.RankCount},RankLevel={item.RankLevel} where RankID={item.RankID}; ");
                }
                int rows = conn.Execute(sql.ToString());
                Console.WriteLine($"成功更新【{rows}/{rankID.Count()}】个【SR】信息({DateTime.Now})");
            }
        }

        private async Task<List<RankInfoModel>> RankInfoListAsync(IEnumerable<string> rankIDs)
        {
            string result = string.Empty;
            List<RankInfoModel> rankInfoList = new List<RankInfoModel>();
            using (HttpClient httpClient = HttpRequestBase.CreateHttpClient())
            {
                httpClient.DefaultRequestHeaders.ConnectionClose = true;
                IEnumerable<List<string>> listGroup = rankIDs.Select((v, i) => new { i = i / 99, v }).GroupBy(n => n.i).Select(g => g.Select(v => v.v).ToList());
                try
                {
                    foreach (List<string> group in listGroup)
                    {
                        string ids = string.Join(",", group);

                        string url = "http://league.speedrunners.doubledutchgames.com/Service/GetRankingList?ids=" + ids;
                        result = await httpClient.HttpGetAsync(url);

                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(int.Parse(SECOND_NUM))); // 延迟5秒
                        if (!string.IsNullOrEmpty(result))
                        {
                            JArray ja = JArray.Parse(result);
                            if (ja.Count() > 0)
                            {
                                foreach (JToken item in ja)
                                {
                                    RankInfoModel rankInfo = new RankInfoModel
                                    {
                                        RankID = JObject.Parse(item.ToString())["id"].ToString(),
                                        RankCount = (int)JObject.Parse(item.ToString())["elo_game_count"],
                                        //RankScore = Convert.ToDouble(JObject.Parse(item.ToString())["elo_rating"])
                                        RankScore = Convert.ToDouble(JObject.Parse(item.ToString())["score"]),
                                        RankLevel = (int)JObject.Parse(item.ToString())["tier"]
                                    };
                                    rankInfoList.Add(rankInfo);
                                }
                            }
                        }
                        else
                        {
                            _log.Log($"天梯分信息查询失败【{group.Count}】个！（{DateTime.Now}）,result:({result})");
                        }
                    }
                    return rankInfoList;
                }
                catch (Exception ex)
                {
                    _log.Log($"报错了！！！！！：{ex}");
                    return null;
                }
            }
        }

        public async System.Threading.Tasks.Task UpdateSteamState()
        {
            try
            {
                if (string.Equals(RUN_UPDATESTEAMINFO, "false", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                List<string> summaryIDs = GetSteamSummaryCandidates();
                List<string> playTimeIDs = GetPlayTimeCandidates();

                using (HttpClient httpClient = HttpRequestBase.CreateHttpClient())
                {
                    await UpdateSteamInfo(summaryIDs, httpClient);
                    await UpdatePlayTime(playTimeIDs, httpClient);
                }
            }
            finally
            {
                Thread.Sleep(SteamUpdateLoopDelaySeconds * 1000);
            }
        }

        private List<string> GetSteamSummaryCandidates()
        {
            int limit = GetCandidateReadLimit(SteamSummaryBatchSize);
            string sql = $@"
                SELECT r.PlatformID
                FROM RankInfo r
                LEFT JOIN (
                    SELECT PlatformID, MAX(LastActiveTime) LastActiveTime
                    FROM AccessToken
                    WHERE LastActiveTime >= DATE_SUB(NOW(), INTERVAL {SteamActiveWindowMinutes} MINUTE)
                    GROUP BY PlatformID
                ) active ON active.PlatformID = r.PlatformID
                WHERE (
                        (active.LastActiveTime IS NOT NULL
                         OR r.LastProfileViewTime >= DATE_SUB(NOW(), INTERVAL {SteamActiveWindowMinutes} MINUTE))
                        AND (r.ModifyTime IS NULL
                             OR r.ModifyTime < DATE_SUB(NOW(), INTERVAL {SteamSummaryActiveTtlMinutes} MINUTE))
                      )
                   OR (r.ModifyTime IS NULL
                       OR r.ModifyTime < DATE_SUB(NOW(), INTERVAL {SteamDailyTtlHours} HOUR))
                ORDER BY
                    CASE
                        WHEN (active.LastActiveTime IS NOT NULL
                              OR r.LastProfileViewTime >= DATE_SUB(NOW(), INTERVAL {SteamActiveWindowMinutes} MINUTE))
                             AND (r.ModifyTime IS NULL
                                  OR r.ModifyTime < DATE_SUB(NOW(), INTERVAL {SteamSummaryActiveTtlMinutes} MINUTE))
                        THEN 0 ELSE 1
                    END,
                    COALESCE(active.LastActiveTime, r.LastProfileViewTime) DESC,
                    COALESCE(r.ModifyTime, r.CreateTime, '1970-01-01') ASC
                LIMIT {limit}";

            using (IDbConnection connection = DbHelper.GetConnection())
            {
                return FilterSteamBackoff(connection.Query<string>(sql).ToList(), SteamSummaryBatchSize);
            }
        }

        private List<string> GetPlayTimeCandidates()
        {
            int limit = GetCandidateReadLimit(SteamPlayTimeBatchSize);
            string sql = $@"
                SELECT r.PlatformID
                FROM RankInfo r
                LEFT JOIN (
                    SELECT PlatformID, MAX(LastActiveTime) LastActiveTime
                    FROM AccessToken
                    WHERE LastActiveTime >= DATE_SUB(NOW(), INTERVAL {SteamActiveWindowMinutes} MINUTE)
                    GROUP BY PlatformID
                ) active ON active.PlatformID = r.PlatformID
                WHERE r.RankType <> 0
                  AND (
                        (
                            active.LastActiveTime IS NOT NULL
                            OR r.LastProfileViewTime >= DATE_SUB(NOW(), INTERVAL {SteamActiveWindowMinutes} MINUTE)
                        )
                        AND (
                            r.PlayTimeModifyTime IS NULL
                            OR r.PlayTimeModifyTime < DATE_SUB(NOW(), INTERVAL {SteamPlayTimeActiveTtlMinutes} MINUTE)
                        )
                      OR (
                            r.PlayTimeModifyTime IS NULL
                            OR r.PlayTimeModifyTime < DATE_SUB(NOW(), INTERVAL {SteamDailyTtlHours} HOUR)
                         )
                  )
                ORDER BY
                    CASE
                        WHEN (active.LastActiveTime IS NOT NULL
                              OR r.LastProfileViewTime >= DATE_SUB(NOW(), INTERVAL {SteamActiveWindowMinutes} MINUTE))
                             AND (r.PlayTimeModifyTime IS NULL
                                  OR r.PlayTimeModifyTime < DATE_SUB(NOW(), INTERVAL {SteamPlayTimeActiveTtlMinutes} MINUTE))
                        THEN 0 ELSE 1
                    END,
                    COALESCE(active.LastActiveTime, r.LastProfileViewTime) DESC,
                    COALESCE(r.PlayTimeModifyTime, r.ModifyTime, r.CreateTime, '1970-01-01') ASC
                LIMIT {limit}";

            using (IDbConnection connection = DbHelper.GetConnection())
            {
                return FilterSteamBackoff(connection.Query<string>(sql).ToList(), SteamPlayTimeBatchSize);
            }
        }

        private async System.Threading.Tasks.Task UpdateSteamInfo(List<string> platformIDs, HttpClient httpClient)
        {
            if (platformIDs == null || !platformIDs.Any())
            {
                return;
            }

            List<PlayersModel> playerInfo = null;
            playerInfo = await BatchRequest(httpClient, platformIDs, 100);
            int rows = 0;
            if (playerInfo.Any())
            {
                playerInfo.ForEach(x => x.ModifyTime = DateTime.Now);

                string setSql = $" ,PersonaName = ?{nameof(PlayersModel.Personaname)} ";
                if (string.Equals(STOP_UPDATE, "true", StringComparison.OrdinalIgnoreCase))
                {
                    setSql = $@" ,PersonaName = (CASE Participate
                                        WHEN 0 THEN ?{nameof(PlayersModel.Personaname)} 
                                        ELSE PersonaName
                                        END) ";
                }
                string sql = $@" UPDATE RankInfo SET AvatarS = ?{nameof(PlayersModel.Avatar)}, 
                                                AvatarM = ?{nameof(PlayersModel.Avatarmedium)}, 
                                                AvatarL = ?{nameof(PlayersModel.Avatarfull)}, 
                                                State = (CASE State WHEN -1 
                                                    THEN -1
                                                    ELSE ?{nameof(PlayersModel.Personastate)} 
                                                    END),
                                                GameID = (CASE State WHEN -1 
                                                    THEN ''
                                                    ELSE ?{nameof(PlayersModel.Gameid)} 
                                                    END),
                                                ModifyTime = ?{nameof(PlayersModel.ModifyTime)} {setSql} 
                                WHERE PlatformID = ?{nameof(PlayersModel.Steamid)}; ";
                using (IDbConnection conn = DbHelper.GetConnection())
                {
                    rows = conn.Execute(sql, playerInfo);
                }
            }

            HashSet<string> updatedIDs = new HashSet<string>(playerInfo.Select(x => x.Steamid));
            MarkSteamFailures(platformIDs.Where(x => !updatedIDs.Contains(x)).ToList());
            if (rows != platformIDs.Count)
            {
                string msg1 = $"更新【{rows}/{platformIDs.Count}】个Steam信息({DateTime.Now})";
                Console.WriteLine(msg1);
            }
        }

        private List<string> FilterSteamBackoff(List<string> platformIDs, int take)
        {
            DateTime now = DateTime.Now;
            return platformIDs
                .Where(x => !_steamFailureUntil.ContainsKey(x) || _steamFailureUntil[x] <= now)
                .Take(take)
                .ToList();
        }

        private void MarkSteamFailures(List<string> platformIDs)
        {
            if (platformIDs == null || !platformIDs.Any())
            {
                return;
            }

            DateTime until = DateTime.Now.AddMinutes(SteamFailureBackoffMinutes);
            foreach (string platformID in platformIDs)
            {
                _steamFailureUntil[platformID] = until;
            }
        }

        private static int GetCandidateReadLimit(int take)
        {
            return Math.Max(take * 3, take);
        }

        private static int GetIntAppSetting(string key, int defaultValue)
        {
            string value = ConfigurationManager.AppSettings[key];
            int parsed;
            if (int.TryParse(value, out parsed) && parsed > 0)
            {
                return parsed;
            }

            return defaultValue;
        }

        private async Task<List<PlayersModel>> BatchRequest<T>(HttpClient httpClient, List<T> PlatformIDs, int count)
        {
            PlatformIDs ??= new List<T>();
            List<T> errIDs = new List<T>();
            IEnumerable<List<T>> listGroup = PlatformIDs.Select((v, i) => new { i = i / count, v }).GroupBy(n => n.i).Select(g => g.Select(v => v.v).ToList());
            List<PlayersModel> playersInfo = new List<PlayersModel>();
            int index = 0;
            foreach (List<T> group in listGroup)
            {
                string steamids = string.Join(",", group);
                string url = "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key=" + ApiKey + "&steamids=" + steamids;
                string result = await httpClient.HttpGetAsync(url);

                //await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(int.Parse(SECOND_NUM))); // 延迟5秒
                if (result == null)
                {
                    errIDs.AddRange(group);
                    continue;
                }
                JObject job = JObject.Parse(result);
                string response = job["response"].ToString();
                if (string.IsNullOrEmpty(response))
                {
                    _log.Log($"查询steam信息【失败】({DateTime.Now})");
                    errIDs.AddRange(group);
                    continue;
                }
                string players = job["response"]["players"].ToString();
                List<PlayersModel> playersList = JsonConvert.DeserializeObject<List<PlayersModel>>(players);
                playersInfo.AddRange(playersList);
                index += playersList.Count;
            }
            return playersInfo;
        }
    }

    internal static class ScheduleExtend
    {
        internal static Schedule ScheduleEx(this Registry reg, LogHelper log, Action job)
        {
            return reg.Schedule(() =>
            {
                try
                {
                    job?.Invoke();
                }
                catch (Exception ex)
                {
                    log.Log(ex.ToString());
                }
            });
        }

        internal static Schedule ScheduleEx(this Registry reg, LogHelper log, Func<System.Threading.Tasks.Task> job)
        {
            return reg.Schedule(() =>
            {
                try
                {
                    System.Threading.Tasks.Task task = job?.Invoke();
                    task?.GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    log.Log(ex.ToString());
                }
            });
        }
    }
}
