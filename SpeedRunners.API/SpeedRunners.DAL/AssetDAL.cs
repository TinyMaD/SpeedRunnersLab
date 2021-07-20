using SpeedRunners.Model;
using SpeedRunners.Model.Asset;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using static Dapper.SqlMapper;

namespace SpeedRunners.DAL
{
    public class AssetDAL : DALBase
    {
        public AssetDAL(DbHelper db) : base(db) { }
        public MPageResult<MMod> GetModList(MModPageParam param)
        {
            StringBuilder where = new StringBuilder()
                .WhereIf(param.ModID.Length > 0, $" AND [ID] IN @{nameof(param.ModID)} ")
                .WhereIf(param.Tag != 0, $" AND [Tag] = @{nameof(param.Tag)} ")
                .WhereIf(!string.IsNullOrWhiteSpace(param.Keywords), $" AND FileName LIKE @{nameof(param.FuzzyKeywords)} ");
            GridReader reader = Db.QueryMultiple($@"SELECT COUNT(ID) FROM Mod WHERE 1 = 1 {where}
SELECT *
FROM(SELECT ROW_NUMBER() OVER(ORDER BY id) AS RowNum, *
 FROM Mod
WHERE 1 = 1 {where}
) AS result
 WHERE RowNum > {(param.PageNo - 1) * param.PageSize} AND RowNum <= {param.PageNo * param.PageSize} ORDER BY ID DESC", param);
            return new MPageResult<MMod>
            {
                Total = reader.ReadFirst<int>(),
                List = reader.Read<MMod>()
            };
        }

        public void AddMod(MMod param)
        {
            Db.Insert("Mod", param, new[] { nameof(param.UploadDate) });
        }

        public void UpdateLikeNum(int modID, int like, int dislike)
        {
            if (like == 0 && dislike == 0) return;
            List<string> strList = new List<string>();
            if (like != 0)
            {
                strList.Add($" [Like] = [Like] + @{nameof(like)} ");
            }
            if (dislike != 0)
            {
                strList.Add($" [dislike] = [dislike] + @{nameof(dislike)} ");
            }
            string updateSql = string.Join(',', strList);
            string sql = $@"UPDATE {updateSql} FROM Mod WHERE [ID] = {modID}";
            Db.Execute(sql, new { like, dislike });
        }

        public void UpdateDownloadNum(int modID)
        {
            string sql = $@"UPDATE Download = Download + 1 FROM Mod WHERE [ID] = {modID}";
            Db.Execute(sql);
        }

        public void AddModStar(int modID, string platformID)
        {
            string sql = $@"INSERT INTO ModStar(ModID, PlatformID) VALUES (@{nameof(modID)}, @{nameof(platformID)})";
            Db.Execute(sql, new { modID, platformID });
        }

        public void DeleteModStar(int modStarID)
        {
            string sql = $@"DELETE ModStar WHERE [ID] = {modStarID}";
            Db.Execute(sql);
        }

        public IEnumerable<int> GetStarMod(string platformID)
        {
            string sql = $@" SELECT * FROM ModStar WHERE platformID = @{nameof(platformID)} ";
            return Db.Query<int>(sql, new { platformID });
        }
    }
}
