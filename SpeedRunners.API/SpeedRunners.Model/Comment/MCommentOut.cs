namespace SpeedRunners.Model.Comment
{
    public class MCommentOut : MComment
    {
        public string PersonaName { get; set; }
        public string AvatarS { get; set; }
        public string ReplyToPersonaName { get; set; }
        public int ReplyCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsLiked { get; set; }
    }
}
