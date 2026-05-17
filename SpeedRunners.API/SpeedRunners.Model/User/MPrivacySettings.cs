using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedRunners.Model.User
{
    public class MPrivacySettings
    {
        public string PlatformID { get; set; }
        /// <summary>
        /// 是否公开 Steam 在线状态的隐私开关：-1=关闭（对外显示离线），0=开启（默认）。
        /// 注意：此字段存于 RankInfo.State 列，与"玩家当前实时在线状态"共用一列；
        /// GetPrivacySettings 中通过 CASE WHEN 把非 -1 的值统一映射成 0，仅用于隐私判断。
        /// </summary>
        public int State { get; set; }
        /// <summary>
        /// 个人主页总开关：0=关闭（其他用户无法访问主页），1=开启（默认）
        /// </summary>
        public int ShowProfile { get; set; }
        /// <summary>
        /// 是否公开最近两周游戏时长：0=隐藏，1=公开（默认）
        /// </summary>
        public int ShowWeekPlayTime { get; set; }
        /// <summary>
        /// 是否允许他人获取本人天梯分相关数据（段位/天梯分/排位场次）：0=禁止，1=允许（默认）
        /// </summary>
        public int RequestRankData { get; set; }
        /// <summary>
        /// 是否公开最近两周新增天梯分：0=隐藏，1=公开（默认）
        /// </summary>
        public int ShowAddScore { get; set; }
        /// <summary>
        /// 是否公开总天梯分（在 SR 总分排行榜中展示）：1=公开，2=不公开
        /// </summary>
        public int RankType { get; set; }
    }
}
