using Microsoft.AspNetCore.Mvc;
using SpeedRunners.BLL;
using SpeedRunners.Model;
using SpeedRunners.Model.User;

namespace SpeedRunners.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotificationController : BaseController<NotificationBLL>
    {
        /// <summary>
        /// 获取消息列表
        /// </summary>
        [User]
        [HttpPost]
        public MPageResult<MNotification> GetList([FromBody] MNotificationQueryParam param)
        {
            return BLL.GetList(BLL.CurrentUser.PlatformID, param);
        }

        /// <summary>
        /// 获取未读消息数量
        /// </summary>
        [User]
        [HttpGet]
        public MUnreadCount GetUnreadCount()
        {
            if (string.IsNullOrEmpty(BLL.CurrentUser?.PlatformID))
            {
                return new MUnreadCount { ReplyCount = 0, LikeCount = 0, TotalCount = 0 };
            }

            return BLL.GetUnreadCount(BLL.CurrentUser.PlatformID);
        }

        /// <summary>
        /// 标记消息为已读
        /// </summary>
        [User]
        [HttpPost]
        public void MarkAsRead([FromBody] MMarkReadParam param)
        {
            BLL.MarkAsRead(BLL.CurrentUser.PlatformID, param);
        }
    }
}
