using System;

namespace SpeedRunners.Model.User
{
    public class MDevice
    {
        public int TokenID { get; set; }
        public string DeviceName { get; set; }
        public DateTime LoginDate { get; set; }
        public DateTime? LastActiveTime { get; set; }
        public bool IsCurrent { get; set; }
        public bool CanLogout { get; set; }
    }
}
