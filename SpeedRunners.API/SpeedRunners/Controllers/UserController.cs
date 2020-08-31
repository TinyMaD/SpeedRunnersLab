using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
using SpeedRunners.Model;
using SpeedRunners.Model.Rank;
using System.Threading.Tasks;

namespace SpeedRunners.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : BaseController<UserBLL>
    {
        [User]
        [HttpGet]
        public MRankInfo GetInfo() => BLL.GetInfo();

        [HttpPost]
        public async Task<MResponse> Login([FromBody] dynamic data)
        {
            string browser = HttpContext.Request?.Headers?["User-Agent"] ?? "未知设备";
            return await BLL.Login(data.query.Value, browser);
        }

        [User]
        [HttpGet("{tokenID}")]
        public MResponse LogoutOther(int tokenID) => BLL.DeleteAccessToken(tokenID);

        [Persona]
        [HttpGet]
        public void LogoutLocal() => BLL.DeleteAccessToken();
    }
}
