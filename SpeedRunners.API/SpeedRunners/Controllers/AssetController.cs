﻿using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
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
        //[User]
        [HttpGet]
        public string GetUploadToken() => BLL.CreateUploadToken();
        [HttpPost]
        public string GetDownloadUrl([FromBody] dynamic data) => BLL.CreateDownloadUrl(data.fileName.Value);
    }
}