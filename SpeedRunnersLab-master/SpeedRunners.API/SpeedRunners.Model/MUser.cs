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
            get => (_rankID == 0 ? ulong.Parse(PlatformID) - 76561197960265728 : _rankID).ToString();
            set
            {
                if (ulong.TryParse(value, out ulong x))
                {
                    _rankID = x;
                }
                _rankID = 0;
            }
        }
        private ulong _rankID;

        public string Browser { get; set; }

        public string Token { get; set; }

        public DateTime LoginDate { get; set; }
    }
}
