using Newtonsoft.Json.Linq;
using SpeedRunners.Model;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunners.DAL
{
    public class UserDAL : DALBase
    {
        private static readonly HashSet<string> AllowedRankInfoCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "State", "RankType"
        };

        private static readonly HashSet<string> AllowedPrivacyCols = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ShowProfile", "ShowWeekPlayTime", "RequestRankData", "ShowAddScore"
        };

        public UserDAL(DbHelper db) : base(db) { }

        public MPrivacySettings GetPrivacySettings(string platformID)
        {
            var exist = Db.ExecuteScalar<int>($"SELECT 1 FROM PrivacySettings WHERE PlatformID = ?{nameof(platformID)}", new { platformID }) > 0;
            if (!exist) {
                Db.Execute($@"INSERT INTO PrivacySettings (PlatformID) VALUES (?{nameof(platformID)}) ", new { platformID });
            }

            // 注意：RankInfo.State 同时承担"实时在线状态"和"是否公开状态的隐私开关"两个语义
            // 这里只需要隐私开关含义，所以把任何非 -1 的值统一映射成 0（参见 MPrivacySettings.State 注释）
            return Db.QueryFirstOrDefault<MPrivacySettings>(
                $@"SELECT
                     a.PlatformID,
                     CASE WHEN a.State = -1 THEN -1
                        ELSE 0 END AS State,
                     a.RankType,
                     IFNULL(b.ShowProfile, 1) ShowProfile,
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
            if (!AllowedRankInfoCols.Contains(colName))
            {
                throw new ArgumentException($"Invalid column name: {colName}", nameof(colName));
            }
            Db.Execute($"UPDATE RankInfo SET {colName} = ?{nameof(value)} WHERE PlatformID = ?{nameof(platformID)}", new { platformID, value });
        }

        public void SetPrivacySettings(string platformID, string colName, int value)
        {
            if (!AllowedPrivacyCols.Contains(colName))
            {
                throw new ArgumentException($"Invalid column name: {colName}", nameof(colName));
            }
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
            return Db.QueryFirstOrDefault<MAccessToken>($"SELECT TokenID, PlatformID, Browser, Token, LoginDate, LastActiveTime FROM AccessToken WHERE Token = ?{nameof(token)}", new { token });
        }

        public MUser GetUserByTokenID(int tokenID)
        {
            return Db.QueryFirstOrDefault<MUser>($"SELECT TokenID, PlatformID, Browser, Token, LoginDate, LastActiveTime FROM AccessToken WHERE TokenID = ?{nameof(tokenID)}", new { tokenID });
        }

        public List<MAccessToken> GetDevicesByPlatformID(string platformID)
        {
            return Db.Query<MAccessToken>($"SELECT TokenID, PlatformID, Browser, Token, LoginDate, LastActiveTime FROM AccessToken WHERE PlatformID = ?{nameof(platformID)} ORDER BY LoginDate DESC", new { platformID }).ToList();
        }

        public void AddAccessToken(MUser user)
        {
            user.LoginDate = DateTime.Now;
            user.LastActiveTime = DateTime.Now;
            Db.Insert("AccessToken", user, new[] { nameof(user.TokenID), nameof(user.RankID) });
        }

        public void RefreshSession(MAccessToken user)
        {
            Db.Execute($"UPDATE AccessToken SET LoginDate = NOW(), LastActiveTime = NOW(), Browser = ?{nameof(user.Browser)} WHERE TokenID = ?{nameof(user.TokenID)} AND PlatformID = ?{nameof(user.PlatformID)}", user);
        }

        public void TouchLastActive(int tokenID)
        {
            Db.Execute($"UPDATE AccessToken SET LastActiveTime = NOW() WHERE TokenID = ?{nameof(tokenID)}", new { tokenID });
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
