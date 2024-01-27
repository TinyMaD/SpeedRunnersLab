using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Scheduler
{
    class PlayersModel
    {
        public string Steamid { get; set; }
        public string Personaname { get; set; }
        public string Profileurl { get; set; }
        public string Avatar { get; set; }
        public string Avatarmedium { get; set; }
        public string Avatarfull { get; set; }
        public int Personastate { get; set; }
        public string Gameid { get; set; }
        public DateTime ModifyTime { get; set; }
    }
}
