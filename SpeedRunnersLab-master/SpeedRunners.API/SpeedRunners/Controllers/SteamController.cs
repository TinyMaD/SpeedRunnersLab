using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
using SpeedRunners.Model.Steam;
using System.Threading.Tasks;

namespace SpeedRunners.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SteamController : BaseController<SteamBLL>
    {
        [HttpGet("{keyWords}")]
        public async Task<MSearchPlayerResult> SearchPlayer(string keyWords) => await BLL.SearchPlayer(keyWords);

        [HttpGet("{userName}/{sessionID}/{pageNo}")]
        public async Task<MSearchPlayerResult> GetPlayerList(string userName, string sessionID, int pageNo) => await BLL.GetPlayerList(userName, sessionID, pageNo);

        [HttpGet("{url}")]
        public async Task<MSearchPlayerResult> SearchPlayerByUrl(string url) => await BLL.SearchPlayerByUrl(url);

        [HttpGet("{steamID64}")]
        public async Task<MSearchPlayerResult> SearchPlayerBySteamID64(string steamID64) => await BLL.SearchPlayerBySteamID64(steamID64);

        [HttpGet]
        public async Task<uint> GetOnlineCount() => await BLL.GetNumberOfCurrentPlayersForGame();
    }
}
