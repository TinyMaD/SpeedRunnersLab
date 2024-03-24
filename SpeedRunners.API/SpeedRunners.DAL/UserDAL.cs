using Newtonsoft.Json.Linq;
using SpeedRunners.Model;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;

namespace SpeedRunners.DAL
{
    public class UserDAL : DALBase
    {
        public UserDAL(DbHelper db) : base(db) { }

        public MPrivacySettings GetPrivacySettings(string platformID)
        {
            var exist = Db.ExecuteScalar<int>($"SELECT 1 FROM PrivacySettings WHERE PlatformID = ?{nameof(platformID)}", new { platformID }) > 0;
            if (!exist) {
                Db.Execute($@"INSERT INTO PrivacySettings (PlatformID) VALUES (?{nameof(platformID)}) ", new { platformID });
            }

            return Db.QueryFirstOrDefault<MPrivacySettings>(
                $@"SELECT
                     a.PlatformID,
                     a.State,
                     a.RankType,
                     IFNULL(b.RequestRankData, 1) RequestRankData,
                     IFNULL(b.ShowAddScore, 1) ShowAddScore,
                     IFNULL(b.ShowWeekPlayTime, 1) ShowWeekPlayTime
                    FROM
                    	RankInfo a
                    LEFT JOIN
                    	PrivacySettings b
                    ON a.PlatformID = b.PlatformID
                    WHERE a.PlatformID = ?{nameof(platformID)}", new { platformID });
        }

        public void SetStateOrRankType(string platformID, string colName, int value)
        {
            Db.Execute($"UPDATE RankInfo SET {colName} = ?{nameof(value)} WHERE PlatformID = ?{nameof(platformID)}", new { platformID, value });
        }

        public void SetPrivacySettings(string platformID, string colName, int value)
        {
            string sql = "";
            if (colName.Equals("RequestRankData", StringComparison.OrdinalIgnoreCase))
            {
                sql = $@", ShowAddScore = ?{nameof(value)} ";
                SetStateOrRankType(platformID, "RankType", value == 1 ? 1 : 2);
            }
            Db.Execute($"UPDATE PrivacySettings SET {colName} = ?{nameof(value)} {sql} WHERE PlatformID = ?{nameof(platformID)}", new { platformID, value });
        }

        public MAccessToken GetUserByToken(string token)
        {
            return Db.QueryFirstOrDefault<MAccessToken>($"SELECT TokenID, PlatformID, Browser, Token, LoginDate, ExToken FROM AccessToken WHERE Token = ?{nameof(token)} OR ExToken = ?{nameof(token)}", new { token });
        }

        public MUser GetUserByTokenID(int tokenID)
        {
            return Db.QueryFirstOrDefault<MUser>($"SELECT TokenID, PlatformID, Browser, Token, LoginDate FROM AccessToken WHERE TokenID = ?{nameof(tokenID)}", new { tokenID });
        }

        public void AddAccessToken(MUser user)
        {
            user.LoginDate = DateTime.Now;
            Db.Insert("AccessToken", user, new[] { nameof(user.TokenID), nameof(user.RankID) });
        }

        public void UpdateAccessToken(MAccessToken user)
        {
            Db.Execute($"UPDATE AccessToken SET Token = ?{nameof(user.Token)}, ExToken = ?{nameof(user.ExToken)}, Browser = ?{nameof(user.Browser)} WHERE TokenID = ?{nameof(user.TokenID)} AND PlatformID = ?{nameof(user.PlatformID)}", user);
        }

        public void DeleteAccessToken(MUser user)
        {
            Db.Execute($"DELETE FROM AccessToken WHERE TokenID = ?{nameof(user.TokenID)} AND PlatformID = ?{nameof(user.PlatformID)}", user);
        }

        public void DeleteAccessToken(string token)
        {
            Db.Execute($"DELETE FROM AccessToken WHERE Token = ?{nameof(token)}", new { token });
        }
    }
}
