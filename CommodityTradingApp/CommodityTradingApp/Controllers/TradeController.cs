using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Controllers
{
    public class TradeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
