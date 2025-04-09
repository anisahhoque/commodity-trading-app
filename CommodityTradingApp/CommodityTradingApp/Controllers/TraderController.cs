using Microsoft.AspNetCore.Mvc;
using CommodityTradingApp.Models;

namespace CommodityTradingApp.Controllers
{
    //Need to ensure i have DBContext, and inject it into controller
    //DBcontext must have class that represents DbSet<Trader> Traders
    public class TraderController : Controller
    {
        // GET: Trader/Index
        // Creating a new instance of the Trader model
        public IActionResult Index()
        {
            Trader trader = new Trader
            {
                Id = 65, //Need a GUID example
                AccountName = "John Doe",
                Balance = 10000.00m
            };
            return View(trader);
        }
    }
}
