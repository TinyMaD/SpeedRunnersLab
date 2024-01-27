using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Scheduler
{
    public class RankInfoModel
    {
        public int WeekPlayTime { get; set; }
        public int PlayTime { get; set; }

        #region Model
        private string _platformid;
        private string _rankid;
        private int? _tmdid;
        private string _personaname;
        private string _avatars;
        private string _avatarm;
        private string _avatarl;
        private int _state;
        private string _gameid;
        private int _ranklevel;
        private int _ranktype;
        private int? _rankcount;
        private double? _rankscore;
        private DateTime? _createtime;
        private DateTime? _modifytime;
        private double? _oldrankscore;
        /// <summary>
        /// 平台ID(steamID64)
        /// </summary>

        public int NO { get; set; }

        public string PlatformID
        {
            set { _platformid = value; }
            get { return _platformid; }
        }
        /// <summary>
        /// 查分ID(steamID3)
        /// </summary>
        public string RankID
        {
            set { _rankid = value; }
            get { return _rankid; }
        }
        public int RankLevel
        {
            set { _ranklevel = value; }
            get { return _ranklevel; }
        }
        /// <summary>
        /// 社区ID
        /// </summary>
        public int? TmdID
        {
            set { _tmdid = value; }
            get { return _tmdid; }
        }
        /// <summary>
        /// 游戏昵称
        /// </summary>
        public string PersonaName
        {
            set { _personaname = value; }
            get { return _personaname; }
        }
        /// <summary>
        /// 小头像
        /// </summary>
        public string AvatarS
        {
            set { _avatars = value; }
            get { return _avatars; }
        }
        /// <summary>
        /// 中头像
        /// </summary>
        public string AvatarM
        {
            set { _avatarm = value; }
            get { return _avatarm; }
        }
        /// <summary>
        /// 大头像
        /// </summary>
        public string AvatarL
        {
            set { _avatarl = value; }
            get { return _avatarl; }
        }
        /// <summary>
        /// 0离线, 1在线, 2忙碌, 3离开, 4打盹, 5 想交易, 6想玩游戏
        /// </summary>
        public int State
        {
            set { _state = value; }
            get { return _state; }
        }
        /// <summary>
        /// 游戏appID(SR: 207140)
        /// </summary>
        public string GameID
        {
            set { _gameid = value; }
            get { return _gameid; }
        }
        /// <summary>
        /// 参与状态;0无游戏,1上榜,2不上榜
        /// </summary>
        public int RankType
        {
            set { _ranktype = value; }
            get { return _ranktype; }
        }
        /// <summary>
        /// 排位局数量
        /// </summary>
        public int? RankCount
        {
            set { _rankcount = value; }
            get { return _rankcount; }
        }
        /// <summary>
        /// 天梯分
        /// </summary>
        public double? RankScore
        {
            set { _rankscore = value; }
            get { return _rankscore; }
        }
        /// <summary>
        /// 旧天梯分
        /// </summary>
        public double? OldRankScore
        {
            set { _oldrankscore = value; }
            get { return _oldrankscore; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? CreateTime
        {
            set { _createtime = value; }
            get { return _createtime; }
        }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? ModifyTime
        {
            set { _modifytime = value; }
            get { return _modifytime; }
        }
        #endregion Model

    }
}
