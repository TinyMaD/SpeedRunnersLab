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
    }
}
