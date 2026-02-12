using System;

namespace SpeedRunners.Model.Comment
{
    public class MComment
    {
        public int ID { get; set; }
        public string PagePath { get; set; }
        public string PlatformID { get; set; }
        public int? ParentID { get; set; }
        public string ReplyToPlatformID { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }
        public int IsDeleted { get; set; }
    }
}
