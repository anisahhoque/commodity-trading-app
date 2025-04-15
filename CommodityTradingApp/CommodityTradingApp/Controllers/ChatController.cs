using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Controllers
{
    public class ChatController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("RocketChatToken");
            var userId = HttpContext.Session.GetString("RocketChatUserId");
            return View();
        }
    }
}
