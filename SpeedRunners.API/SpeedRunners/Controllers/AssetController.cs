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

        [HttpPost]
        public MPageResult<MMod> GetModList([FromBody] MModPageParam param) => BLL.GetModList(param);

        [User]
        [HttpPost]
        public void AddMod([FromBody] MMod param) => BLL.AddMod(param);
    }
}
