using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
using SpeedRunners.Model;
using SpeedRunners.Model.Asset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpeedRunners.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AssetController : BaseController<AssetBLL>
    {
        [User]
        [HttpGet]
        public string[] GetUploadToken() => BLL.CreateUploadToken();

        [User]
        [HttpPost]
        public string GetDownloadUrl([FromBody] dynamic data) => BLL.CreateDownloadUrl(data.fileName.Value);

        [User]
        [HttpPost]
        public Task<MResponse> DeleteMod([FromBody] MDeleteMod data) => BLL.DeleteMod(data);

        [Persona]
        [HttpPost]
        public MPageResult<MModOut> GetModList([FromBody] MModPageParam param) => BLL.GetModList(param);

        [User]
        [HttpPost]
        public void AddMod([FromBody] MMod param) => BLL.AddMod(param);

        [User]
        [HttpGet("{modID}/{star}")]
        public void OperateModStar(int modID, bool star) => BLL.OperateModStar(modID, star);

        [HttpGet]
        public Task<MResponse> GetAfdianSponsor() => BLL.GetAfdianSponsorAsync();
    }
}
