using System;

namespace SpeedRunners.Model.Comment
{
    public class MCommentLike
    {
        public int ID { get; set; }
        public int CommentID { get; set; }
        public string PlatformID { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
