using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SpeedRunners.Model.Steam
{
    public class MSearchPlayerResult
    {
        /// <summary>
        /// 是否返回游戏信息；True游戏信息，False玩家列表
        /// </summary>
        public bool IsGameInfo { get; set; }
        /// <summary>
        /// 游戏信息
        /// </summary>
        public JToken GameInfo { get; set; }
        public int Total { get; set; }
        public int PageNo { get; set; }
        public string SessionID { get; set; }
        /// <summary>
        /// 玩家列表
        /// </summary>
        public List<PlayerInfo> PlayerList { get; set; }
    }
    public class PlayerInfo
    {
        public string Avatar { get; set; }
        public string UserName { get; set; }
        /// <summary>
        /// 唯一标识类型Profiles/ID
        /// </summary>
        public string ProfilesOrID { get; set; }
        /// <summary>
        /// 玩家唯一CustomUrl或SteamID64
        /// </summary>
        public string ContentOfID { get; set; }
    }
}
