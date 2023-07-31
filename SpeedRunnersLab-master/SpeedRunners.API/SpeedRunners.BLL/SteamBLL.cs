using Newtonsoft.Json.Linq;
using SpeedRunners.Model.Rank;
using SpeedRunners.Model.Steam;
using SpeedRunners.Utils;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpeedRunners.BLL
{
    public class SteamBLL : BaseBLL
    {
        private static string BaseUrl => "https://api.steampowered.com";
        private static string ApiKey => AppSettings.GetConfig("ApiKey");
        private static uint AppId => 207140;
        private static SteamWebInterfaceFactory WebInterfaceFactory => new SteamWebInterfaceFactory(ApiKey);

        /// <summary>
        /// 获取全球游戏中人数
        /// </summary>
        /// <returns></returns>
        public async Task<uint> GetNumberOfCurrentPlayersForGame()
        {
            SteamUserStats iSteamUserStats = WebInterfaceFactory.CreateSteamWebInterface<SteamUserStats>();
            ISteamWebResponse<uint> response = await iSteamUserStats.GetNumberOfCurrentPlayersForGameAsync(AppId);
            return response.Data;
        }

        /// <summary>
        /// 获取最近游戏信息
        /// </summary>
        /// <param name="steamID"></param>
        /// <returns></returns>
        public async Task<RecentlyPlayedGamesResultModel> GetRecentlyPlayedGames(string steamID)
        {
            PlayerService iPlayer = WebInterfaceFactory.CreateSteamWebInterface<PlayerService>();
            ISteamWebResponse<RecentlyPlayedGamesResultModel> response = await iPlayer.GetRecentlyPlayedGamesAsync(ulong.Parse(steamID));
            return response.Data;
        }

        /// <summary>
        /// 获取SR天梯分
        /// </summary>
        /// <param name="rankIDs"></param>
        /// <returns></returns>
        public async Task<List<MRankInfo>> GetRankScore(IEnumerable<string> rankIDs)
        {
            List<MRankInfo> rankInfoList = new List<MRankInfo>();
            string ids = string.Join(',', rankIDs);
            string url = "http://league.speedrunners.doubledutchgames.com/Service/GetRankingList?ids=" + ids;
            string result = await HttpHelper.HttpGet(url);
            if (result.Contains("score"))
            {
                JArray ja = JArray.Parse(result);
                if (ja.Count() > 0)
                {
                    foreach (JToken item in ja)
                    {
                        MRankInfo rankInfo = new MRankInfo
                        {
                            RankID = JObject.Parse(item.ToString())["id"].ToString(),
                            RankCount = (int)JObject.Parse(item.ToString())["elo_game_count"],
                            //RankScore = Convert.ToDouble(JObject.Parse(item.ToString())["elo_rating"])
                            RankScore = Convert.ToDecimal(JObject.Parse(item.ToString())["score"]),
                            RankLevel = (int)JObject.Parse(item.ToString())["tier"]
                        };
                        rankInfoList.Add(rankInfo);
                    }
                }
            }
            return rankInfoList;
        }

        /// <summary>
        /// 获取Steam用户信息
        /// </summary>
        /// <param name="steamID"></param>
        /// <returns></returns>
        public async Task<PlayerSummaryModel> GetPlayerSummary(string steamID)
        {
            SteamUser iUser = WebInterfaceFactory.CreateSteamWebInterface<SteamUser>();
            ISteamWebResponse<PlayerSummaryModel> response = await iUser.GetPlayerSummaryAsync(ulong.Parse(steamID));
            return response.Data;
        }

        /// <summary>
        /// 获取用户游戏
        /// </summary>
        /// <param name="steamID"></param>
        /// <returns></returns>
        public async Task<OwnedGamesResultModel> GetOwnedGames(string steamID)
        {
            PlayerService iPlayerService = WebInterfaceFactory.CreateSteamWebInterface<PlayerService>();
            ISteamWebResponse<OwnedGamesResultModel> response = await iPlayerService.GetOwnedGamesAsync(ulong.Parse(steamID));
            return response.Data;
        }

        /// <summary>
        /// 查询游戏信息
        /// </summary>
        /// <param name="keyWords"></param>
        /// <returns></returns>
        public async Task<MSearchPlayerResult> SearchPlayer(string keyWords)
        {
            Task<MSearchPlayerResult> steamID64Task = SearchPlayerBySteamID64(keyWords);
            Task<MSearchPlayerResult> customURLTask = SearchPlayerByUrl(keyWords);
            Task<MSearchPlayerResult> userListTask = SearchUsers(keyWords);
            List<Task<MSearchPlayerResult>> taskList = new List<Task<MSearchPlayerResult>> { steamID64Task, customURLTask, userListTask };
            MSearchPlayerResult result = new MSearchPlayerResult();
            while (taskList.Count > 0)
            {
                Task<MSearchPlayerResult> finishedTask = await Task.WhenAny(taskList);
                MSearchPlayerResult response = await finishedTask;
                if (response?.IsGameInfo ?? false)
                {
                    return response;
                }
                if (finishedTask == userListTask)
                {
                    result = response;
                }
                taskList.Remove(finishedTask);
            }
            return result;
        }

        /// <summary>
        /// 通过昵称+SessionID+PageNo搜索玩家
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="sessionID"></param>
        /// <param name="pageNo"></param>
        /// <returns></returns>
        public async Task<MSearchPlayerResult> GetPlayerList(string userName, string sessionID, int pageNo)
        {
            string httpUrl = "https://steamcommunity.com/search/SearchCommunityAjax?text=" + userName + $"&filter=users&sessionid={sessionID}&steamid_user=false&page={pageNo}";
            CookieContainer cookie = new CookieContainer(); //创建Cookie容器对象 （Cookies集合）
            cookie.Add(new Uri("https://steamcommunity.com/"), new Cookie("sessionid", sessionID));
            cookie.Add(new Uri("https://steamcommunity.com/"), new Cookie("steamCountry", "CN%7C65c3258fe7f771ce24de930fa87b1495"));
            string response;
            try
            {
                response = await HttpHelper.HttpGet(httpUrl, cookie);
            }
            catch (Exception)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }
            JObject obj = JObject.Parse(response);
            if ((int)obj["success"] != 1)
            {
                return null;
            }
            MSearchPlayerResult result = new MSearchPlayerResult
            {
                Total = (int)obj["search_result_count"],
                PageNo = (int)obj["search_page"],
                SessionID = sessionID
            };
            string html = obj["html"].ToString();
            MatchCollection avatorList = Regex.Matches(html, @"(?<=img\s?src\s?=\s?"")[^>]*(?="">)", RegexOptions.IgnoreCase);
            MatchCollection nameList = Regex.Matches(html, @"(?<=>)((?!</a>).)*(?=</a><br\s{1}/>)", RegexOptions.IgnoreCase);
            MatchCollection contentID = Regex.Matches(html, @"(?<=<a\s{1}href=""https://steamcommunity.com/)((?!><img).)*(?="">)", RegexOptions.IgnoreCase);
            List<PlayerInfo> playerList = new List<PlayerInfo>();
            for (int i = 0; i < avatorList.Count; i++)
            {
                string[] contentOfID = contentID?[i]?.Value.Split("/");
                playerList.Add(new PlayerInfo
                {
                    Avatar = avatorList[i]?.Value,
                    UserName = nameList[i]?.Value,
                    ProfilesOrID = contentOfID?[0],
                    ContentOfID = contentOfID?[1]
                });
            }
            result.PlayerList = playerList;
            return result;
        }

        /// <summary>
        /// 通过CustomURL查询游戏信息
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<MSearchPlayerResult> SearchPlayerByUrl(string url)
        {
            string steamID64 = await GetSteamID64ByUrl(url);
            if (string.IsNullOrWhiteSpace(steamID64))
            {
                return null;
            }
            return await SearchPlayerBySteamID64(steamID64);
        }

        /// <summary>
        /// 通过SteamID64查询游戏信息
        /// </summary>
        /// <param name="steamID64"></param>
        /// <returns></returns>
        public async Task<MSearchPlayerResult> SearchPlayerBySteamID64(string steamID64)
        {
            string httpUrl = $"{BaseUrl}/ISteamUserStats/GetUserStatsForGame/v2/?key={ApiKey}&steamid={steamID64}&appid={AppId}";
            string response;
            try
            {
                response = await HttpHelper.HttpGet(httpUrl);
            }
            catch (Exception)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }
            JObject obj = JObject.Parse(response);
            return new MSearchPlayerResult
            {
                IsGameInfo = true,
                GameInfo = ToChinese(obj["playerstats"])
            };
        }

        #region 私有接口
        /// <summary>
        /// 通过CustomURL查询SteamID64
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<string> GetSteamID64ByUrl(string url)
        {
            string httpUrl = $"{BaseUrl}/ISteamUser/ResolveVanityURL/v0001/?key={ApiKey}&vanityurl={url}";
            string response;
            try
            {
                response = await HttpHelper.HttpGet(httpUrl);
            }
            catch (Exception)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }
            JObject obj = JObject.Parse(response);
            string success = obj["response"]["success"].ToString();
            if (success != "1")
            {
                return null;
            }
            return obj["response"]["steamid"].ToString();
        }

        /// <summary>
        /// 通过昵称搜索玩家
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private async Task<MSearchPlayerResult> SearchUsers(string userName)
        {
            string sessionID = await GetSessionID(userName);
            if (string.IsNullOrWhiteSpace(sessionID))
            {
                return null;
            }
            return await GetPlayerList(userName, sessionID, 1);
        }

        /// <summary>
        /// 获取通过昵称搜索玩家时用到的SessionID
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetSessionID(string userName)
        {
            string httpUrl = "https://steamcommunity.com/search/users/#text=" + userName;
            string response;
            try
            {
                response = await HttpHelper.HttpGet(httpUrl);
            }
            catch (Exception)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(response))
            {
                return null;
            }
            return Regex.Match(response, "(?<=g_sessionID\\s?=\\s?\")\\w*(?=\")", RegexOptions.IgnoreCase).Value;
        }

        /// <summary>
        /// 游戏信息翻译为中文
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private JToken ToChinese(JToken data)
        {
            JArray stats = (JArray)data["stats"];
            foreach (JToken item in stats)
            {
                switch (item["name"].ToString())
                {
                    case "weapon_hook_shot":
                        item["name"] = "发射金钩";
                        break;
                    case "weapon_hook_hit":
                        item["name"] = "金钩命中";
                        break;
                    case "weapon_hook_dodged":
                        item["name"] = "躲避金钩";
                        break;
                    case "weapon_bomb_planted":
                        item["name"] = "安置炸弹";
                        break;
                    case "weapon_bomb_victims":
                        item["name"] = "炸弹受害者";
                        break;
                    case "weapon_bomb_suicides":
                        item["name"] = "炸弹自杀";
                        break;
                    case "weapon_shockwave_fired":
                        item["name"] = "发射冲击波";
                        break;
                    case "weapon_shockwave_obstacles_removed":
                        item["name"] = "冲击波击落道具";
                        break;
                    case "weapon_shockwave_victims":
                        item["name"] = "冲击波受害者";
                        break;
                    case "weapon_crate_dropped":
                        item["name"] = "扔箱子个数";
                        break;
                    case "weapon_crate_suicide":
                        item["name"] = "撞自己的箱子";
                        break;
                    case "weapon_crate_victims":
                        item["name"] = "箱子受害者";
                        break;
                    case "weapon_crate_air_victims":
                        item["name"] = "空中箱子击中";
                        break;
                    case "weapon_crate_rocket_victims":
                        item["name"] = "箱子挡火箭";
                        break;
                    case "weapon_rocket_shot":
                        item["name"] = "发射火箭";
                        break;
                    case "weapon_rocket_victims":
                        item["name"] = "火箭击中";
                        break;
                    case "weapon_rocket_suicide":
                        item["name"] = "火箭自杀";
                        break;
                    case "weapon_drill_used":
                        item["name"] = "使用钻头";
                        break;
                    case "weapon_drill_victims":
                        item["name"] = "钻头受害者";
                        break;
                    case "weapon_drill_obstacles_removed":
                        item["name"] = "钻头清除障碍";
                        break;
                    case "game_wins":
                        item["name"] = "胜场";
                        break;
                    case "game_losses":
                        item["name"] = "败场";
                        break;
                    case "speed_avg":
                        item["name"] = "平均速度";
                        break;
                    case "distance_traveled":
                        item["name"] = "奔跑距离";
                        break;
                    case "slide_tackles":
                        item["name"] = "铲人次数";
                        break;
                    case "jumps":
                        item["name"] = "跳跃次数";
                        break;
                    case "obstacles_hit":
                        item["name"] = "障碍物击中";
                        break;
                    case "died":
                        item["name"] = "致命死亡";
                        break;
                    case "hooks_shot":
                        item["name"] = "发射蓝钩";
                        break;
                    case "hooks_hit":
                        item["name"] = "钩到白色天花板";
                        break;
                    case "slide_tackles_in_air":
                        item["name"] = "空中铲人";
                        break;
                    case "overtakes":
                        item["name"] = "超越对手";
                        break;
                    case "game_wins_character_0":
                        item["name"] = "使用Speedrunner获胜";
                        break;
                    case "game_wins_character_1":
                        item["name"] = "使用Unic获胜";
                        break;
                    case "game_wins_character_2":
                        item["name"] = "使用Cosmonaut获胜";
                        break;
                    case "game_wins_character_3":
                        item["name"] = "使用Hothead获胜";
                        break;
                    case "game_wins_character_4":
                        item["name"] = "使用Moonraker获胜";
                        break;
                    case "game_wins_team_0":
                        item["name"] = "YT KoS Wins w/ Team Falcon";
                        break;
                    case "game_wins_team_1":
                        item["name"] = "YT KoS Wins w/ Team SR";
                        break;
                    case "blocked_bullet_hook":
                        item["name"] = "箱子挡钩子";
                        break;
                }
            }
            data["stats"] = stats;
            return data;
        }
        #endregion
    }
}
