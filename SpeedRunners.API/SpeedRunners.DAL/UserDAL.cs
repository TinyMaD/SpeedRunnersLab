using SpeedRunners.Model;
using SpeedRunners.Model.User;
using SpeedRunners.Utils;
using System;

namespace SpeedRunners.DAL
{
    public class UserDAL : DALBase
    {
        public UserDAL(DbHelper db) : base(db) { }

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
            Db.Execute($"DELETE AccessToken WHERE TokenID = ?{nameof(user.TokenID)} AND PlatformID = ?{nameof(user.PlatformID)}", user);
        }

        public void DeleteAccessToken(string token)
        {
            Db.Execute($"DELETE AccessToken WHERE Token = ?{nameof(token)}", new { token });
        }
    }
}
