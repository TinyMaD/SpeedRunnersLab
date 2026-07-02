using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SpeedRunners.BLL;
using SpeedRunners.Model.Asset;
using SpeedRunners.Model.Rank;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace SpeedRunners.Middleware
{
    public class SitemapMiddleware
    {
        private readonly RequestDelegate _next;
        private const string Origin = "https://www.speedrunners.cn";

        public SitemapMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.Equals("/api/sitemap", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            RankBLL rankBLL = context.RequestServices.GetRequiredService<RankBLL>();
            AssetBLL assetBLL = context.RequestServices.GetRequiredService<AssetBLL>();
            List<MRankInfo> players = rankBLL.GetRankList();
            List<MModOut> mods = GetAllMods(assetBLL);

            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            foreach (string path in new[] { "/", "/rank", "/mod", "/match", "/log", "/searchplayer" })
            {
                AppendUrl(sb, Origin + path, null, "daily", "0.8");
            }
            foreach (MRankInfo player in players.Where(x => !string.IsNullOrWhiteSpace(x.PlatformID)))
            {
                AppendUrl(sb, $"{Origin}/profile/{player.PlatformID}", player.ModifyTime, "weekly", "0.6");
            }
            foreach (MModOut mod in mods)
            {
                AppendUrl(sb, $"{Origin}/mod/{mod.ID}", mod.UploadDate, "monthly", "0.5");
            }
            sb.Append("</urlset>");

            context.Response.ContentType = "application/xml; charset=utf-8";
            context.Response.Headers["Cache-Control"] = "public, max-age=3600";
            await context.Response.WriteAsync(sb.ToString());
        }

        private static List<MModOut> GetAllMods(AssetBLL assetBLL)
        {
            var mods = new Dictionary<int, MModOut>();
            for (int tag = 0; tag <= 6; tag++)
            {
                IEnumerable<MModOut> list = assetBLL.GetModList(new MModPageParam
                {
                    Tag = tag,
                    PageNo = 1,
                    PageSize = 10000
                }).List ?? Enumerable.Empty<MModOut>();

                foreach (MModOut mod in list)
                {
                    mods[mod.ID] = mod;
                }
            }
            return mods.Values.OrderByDescending(x => x.UploadDate).ToList();
        }

        private static void AppendUrl(StringBuilder sb, string loc, DateTime? lastmod, string freq, string priority)
        {
            sb.Append("<url><loc>").Append(SecurityElement.Escape(loc)).Append("</loc>");
            if (lastmod.HasValue)
            {
                sb.Append("<lastmod>").Append(lastmod.Value.ToString("yyyy-MM-dd")).Append("</lastmod>");
            }
            sb.Append("<changefreq>").Append(freq).Append("</changefreq>");
            sb.Append("<priority>").Append(priority).Append("</priority></url>");
        }
    }
}
