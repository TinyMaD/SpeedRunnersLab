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
        private static readonly LogHelper _log = LogHelper.GetCurrentClassLogHelper();
        private string _lastID;
        public async void Execute()
        {
            try
            {
                //await UpdateSteamInfo();
                //await UpdatePlayTime();
                //await UpdateScore();

                Registry timer = new Registry();
                _ = timer.ScheduleEx(_log, delegate
                {
                    UpdateScore();
                }).ToRunNow().AndEvery(10)
                    .Minutes();

                _ = timer.ScheduleEx(_log, delegate
                {
                    UpdateOldScore();
                }).ToRunEvery(1).Days()
                    .At(18, 0);

                timer.ScheduleEx(_log, delegate
                {
                    _ = UpdatePlayTime();
                }).ToRunEvery(1).Days()
                    .At(5, 0);

                _ = timer.ScheduleEx(_log, delegate
                {
                    InsertRankLog();
                }).ToRunEvery(1).Days()
                    .At(17, 30);

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

        private async System.Threading.Tasks.Task UpdatePlayTime()
        {
            using (HttpClient httpClient = HttpRequestBase.CreateHttpClient())
            {
                List<string> idList = new List<string>();
                using (IDbConnection conn = DbHelper.GetConnection())
                {
                    Console.WriteLine($"更新【两周游戏时间】({DateTime.Now})");
                    idList = conn.Query<string>("select PlatformID from RankInfo where RankType<>0").ToList();
                }
                List<RankInfoModel> rankParamList = new List<RankInfoModel>();
                foreach (string platformID in idList)
                {
                    string url = "https://api.steampowered.com/IPlayerService/GetRecentlyPlayedGames/v1/?key=" + ApiKey + $"&steamid={platformID}";
                    string res = await httpClient.HttpGetAsync(url);
                    await System.Threading.Tasks.Task.Delay(500); // 延迟0.5秒
                    int weekTime = 0;
                    int time = 0;
                    if (!string.IsNullOrEmpty(res))
                    {
                        JToken jt = JObject.Parse(res)["response"];
                        if (jt["total_count"] != null && (int)jt["total_count"] > 0)
                        {
                            JArray games = JArray.Parse(jt["games"].ToString());
                            foreach (JToken g in games)
                            {
                                if (g["appid"].ToString() == "207140")
                                {
                                    weekTime = (int)g["playtime_2weeks"];
                                }
                            }
                        }
                    }
                    string url2 = "https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key=" + ApiKey + $"&steamid={platformID}";
                    string res2 = await httpClient.HttpGetAsync(url2);
                    if (!string.IsNullOrEmpty(res2))
                    {
                        JToken jt = JObject.Parse(res2)["response"];
                        if (jt["game_count"] != null && (int)jt["game_count"] > 0)
                        {
                            JArray games = JArray.Parse(jt["games"].ToString());
                            foreach (JToken g in games)
                            {
                                if (g["appid"].ToString() == "207140")
                                {
                                    time = (int)g["playtime_forever"];
                                }
                            }
                        }
                    }
                    rankParamList.Add(new RankInfoModel
                    {
                        PlatformID = platformID,
                        PlayTime = time,
                        WeekPlayTime = weekTime,
                    });
                }
                using (IDbConnection conn = DbHelper.GetConnection())
                {
                    conn.Execute($"update RankInfo set WeekPlayTime=?{nameof(RankInfoModel.WeekPlayTime)}, PlayTime=?{nameof(RankInfoModel.PlayTime)} where PlatformID=?{nameof(RankInfoModel.PlatformID)}", rankParamList);
                }
                Console.WriteLine($"【两周游戏时间】更新结束({DateTime.Now})");
            }
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
            List<string> platformIDs = new List<string>();
            using (IDbConnection connection = DbHelper.GetConnection())
            {
                platformIDs = connection.Query<string>("select PlatformID from RankInfo ORDER BY PlatformID").ToList();
            }

            int count = platformIDs.Count / (int.Parse(MINUTES_NUM) * 60 / 30);

            platformIDs.AddRange(platformIDs);// platformIDs x2;

            var cursor = platformIDs.FindIndex(x => x == _lastID);

            var paramList = platformIDs.GetRange(cursor + 1, count);

            _lastID = paramList.LastOrDefault();

            await UpdateSteamInfo(paramList);

            Thread.Sleep(30 * 1000);
        }

        private async System.Threading.Tasks.Task UpdateSteamInfo(List<string> platformIDs)
        {
            if (RUN_UPDATESTEAMINFO == "false") return;

            List<PlayersModel> playerInfo = null;
            using (HttpClient httpClient = HttpRequestBase.CreateHttpClient())
            {
                playerInfo = await BatchRequest(httpClient, platformIDs, 100);
            }
            int rows = 0;
            if (playerInfo.Any())
            {
                playerInfo.ForEach(x => x.ModifyTime = DateTime.Now);

                string setSql = $" ,PersonaName = ?{nameof(PlayersModel.Personaname)} ";
                if (STOP_UPDATE.Equals("true"))
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
            if (rows != platformIDs.Count)
            {
                string msg1 = $"更新【{rows}/{platformIDs.Count}】个Steam信息({DateTime.Now})";
                Console.WriteLine(msg1);
            }
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
    }
}
