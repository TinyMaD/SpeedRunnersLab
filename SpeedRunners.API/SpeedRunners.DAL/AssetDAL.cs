using SpeedRunners.Model;
using SpeedRunners.Model.Asset;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Dapper.SqlMapper;

namespace SpeedRunners.DAL
{
    public class AssetDAL : DALBase
    {
        public AssetDAL(DbHelper db) : base(db) { }
        public MPageResult<MMod> GetModList(MModPageParam param, string currentUserID)
        {
            StringBuilder where = new StringBuilder()
                .WhereIf(true, $" AND `Tag` = ?{nameof(param.Tag)} ")
                .WhereIf(!string.IsNullOrWhiteSpace(param.Keywords), $" AND Title LIKE ?{nameof(param.FuzzyKeywords)} ");
            var starModIDs = new List<int>();
            if (!string.IsNullOrWhiteSpace(currentUserID))
            {
                string sql = $@" SELECT ModID FROM ModStar WHERE platformID = {currentUserID} ";
                starModIDs = Db.Query<int>(sql).ToList();

                if (param.OnlyStar)
                {
                    string starModIDsStr = string.Join(',', starModIDs);
                    where.Append($" AND `ID` IN ({starModIDsStr}) ");
                }
            }
            GridReader reader = Db.QueryMultiple($@"SELECT COUNT(ID) FROM `Mod` WHERE 1 = 1 {where};
SELECT *
 FROM `Mod`
WHERE 1 = 1 {where} 
ORDER BY ID DESC
LIMIT ?{nameof(param.Offset)}, ?{nameof(param.PageSize)}; ", param);
            var result = new MPageResult<MMod>
            {
                Total = reader.ReadFirst<int>(),
                List = reader.Read<MMod>()
            };
            var modIDs = result.List.Select(x => x.ID);
            var starCountList = Db.Query<(int, int)>($@"
SELECT ModID, COUNT(ModID)
FROM ModStar 
WHERE ModID IN ?{nameof(modIDs)}
GROUP BY ModID"
, new { modIDs });
            foreach (var item in result.List)
            {
                var (modID, starCount) = starCountList.FirstOrDefault(x => x.Item1 == item.ID);
                if (modID != 0)
                {
                    item.StarCount = starCount;
                }
                if (!string.IsNullOrWhiteSpace(currentUserID))
                {
                    if (starModIDs.Any(x => x == item.ID))
                    {
                        item.Star = true;
                    }
                }
            }
            return result;
        }

        public void AddMod(MMod param)
        {
            string sql = $@"SELECT 1 FROM `Mod` WHERE imgUrl = ?{nameof(param.ImgUrl)}";
            bool exist = Db.ExecuteScalar<int>(sql, param) == 1;
            if (exist) return;

            param.UploadDate = DateTime.Now;
            Db.Insert("Mod", param, new[] { nameof(param.ID), nameof(param.Star), nameof(param.StarCount) });
        }

        public void UpdateLikeNum(int modID, int like, int dislike)
        {
            if (like == 0 && dislike == 0) return;
            List<string> strList = new List<string>();
            if (like != 0)
            {
                strList.Add($" `Like` = `Like` + ?{nameof(like)} ");
            }
            if (dislike != 0)
            {
                strList.Add($" `dislike` = `dislike` + ?{nameof(dislike)} ");
            }
            string updateSql = string.Join(',', strList);
            string sql = $@"UPDATE {updateSql} FROM `Mod` WHERE `ID` = {modID}";
            Db.Execute(sql, new { like, dislike });
        }

        public void UpdateDownloadNum(string key)
        {
            string sql = $@"UPDATE `Mod` SET Download = Download + 1 WHERE `FileUrl` = ?{nameof(key)}";
            Db.Execute(sql, new { key });
        }

        public void AddModStar(int modID, string platformID)
        {
            string sql = $@"INSERT INTO ModStar(ModID, PlatformID) VALUES (?{nameof(modID)}, ?{nameof(platformID)})";
            Db.Execute(sql, new { modID, platformID });
        }

        public void DeleteModStar(int modStarID, string currentUserID)
        {
            string sql = $@"DELETE FROM ModStar WHERE `ModID` = {modStarID} AND PlatformID = {currentUserID}";
            Db.Execute(sql);
        }
    }
}
