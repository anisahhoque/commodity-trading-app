using Microsoft.AspNetCore.Mvc;
using CommodityTradingApp.Models;

namespace CommodityTradingApp.Controllers
{
    public class TraderController : Controller
    {
        // GET: Trader/Index
        // Creating a new instance of the Trader model
        public IActionResult Index()
        {
            Trader trader = new Trader
            {
                Id = 1,
                AccountName = "John Doe",
                Balance = 10000.00m
            };
            return View(trader);
        }
    }
}
