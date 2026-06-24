using System;

namespace SpeedRunners.Model
{
    /// <summary>
    /// 当前用户实体
    /// </summary>
    public class MUser
    {
        public int TokenID { get; set; }

        public string PlatformID { get; set; }

        public string RankID
        {
            get
            {
                if (_rankID != 0)
                {
                    return _rankID.ToString();
                }
                if (ulong.TryParse(PlatformID, out ulong platformID))
                {
                    return (platformID - 76561197960265728).ToString();
                }
                return "0";
            }
            set
            {
                _rankID = ulong.TryParse(value, out ulong x) ? x : 0;
            }
        }
        private ulong _rankID;

        public string Browser { get; set; }

        public string Token { get; set; }

        public DateTime LoginDate { get; set; }

        public DateTime? LastActiveTime { get; set; }
    }
}
