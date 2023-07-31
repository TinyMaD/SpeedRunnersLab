using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Model.User
{
    public class MAccessToken
    {
        public int TokenID { get; set; }
        public string PlatformID { get; set; }
        public string Browser { get; set; }
        public string Token { get; set; }
        public DateTime LoginDate { get; set; }
        public string ExToken { get; set; }
    }
}
