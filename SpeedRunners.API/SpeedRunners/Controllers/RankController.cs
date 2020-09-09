using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
using SpeedRunners.Model;
using SpeedRunners.Model.Rank;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpeedRunners.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class RankController : BaseController<RankBLL>
    {
        [Persona]
        [HttpGet]
        public List<MRankInfo> GetRankList() => BLL.GetRankList();

        [HttpGet]
        public List<MRankInfo> GetAddedChart() => BLL.GetAddedChart();

        [Persona]
        [HttpGet]
        public List<MRankInfo> GetHourChart() => BLL.GetHourChart();

        [User]
        [HttpGet]
        public async Task<MResponse> AsyncSRData() => await BLL.AsyncSRData();

        [User]
        [HttpGet]
        public async Task InitUserData() => await BLL.InitUserData();

        [Persona]
        [HttpGet]
        public List<MRankInfo> GetPlaySRList() => BLL.GetPlaySRList();
    }
}