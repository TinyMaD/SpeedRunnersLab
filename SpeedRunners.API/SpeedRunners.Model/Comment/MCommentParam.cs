namespace SpeedRunners.Model.Comment
{
    public class MCommentPageParam : MPageParam
    {
        public string PagePath { get; set; }
        public int? ParentID { get; set; }
        public string CurrentUserPlatformID { get; set; }
    }

    public class MAddComment
    {
        public string PagePath { get; set; }
        public int? ParentID { get; set; }
        public string ReplyToPlatformID { get; set; }
        public string Content { get; set; }
    }
}
