using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Controllers
{
    public class ChatController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}
