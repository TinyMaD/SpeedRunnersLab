using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
using SpeedRunners.Model;
using SpeedRunners.Model.Comment;

namespace SpeedRunners.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : BaseController<CommentBLL>
    {
        [Persona]
        [HttpPost]
        public MPageResult<MCommentOut> GetCommentList([FromBody] MCommentPageParam param)
            => BLL.GetCommentList(param);

        [User]
        [HttpPost]
        public void AddComment([FromBody] MAddComment param)
            => BLL.AddComment(param);

        [User]
        [HttpGet("{commentID}")]
        public void DeleteComment(int commentID)
            => BLL.DeleteComment(commentID);

        [User]
        [HttpGet("{commentID}")]
        public int ToggleLike(int commentID)
            => BLL.ToggleLike(commentID);
    }
}
