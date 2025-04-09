using Microsoft.AspNetCore.Mvc;
using CommodityTradingApp.Models;

namespace CommodityTradingApp.Controllers
{
    //Need to ensure i have DBContext, and inject it into controller
    //DBcontext must have class that represents DbSet<Trader> Traders


    public class TraderController : Controller
    {
        // GET: Trader/Index
        public IActionResult Index()
        {
            // Simulate current user and role
            bool isManager = true; // Change to false to test user view
            int currentUserId = 1;

            var allTraders = new List<Trader>
    {
        new Trader { Id = Guid.NewGuid(), AccountName = "Alice", Balance = 5000, UserId = 1 },
        new Trader { Id = Guid.NewGuid(), AccountName = "Bob", Balance = 10000, UserId = 2 },
        new Trader { Id = Guid.NewGuid(), AccountName = "Charlie", Balance = 7500, UserId = 3 }
    };

            IEnumerable<Trader> visibleTraders;

            if (isManager)
            {
                visibleTraders = allTraders;
            }
            else
            {
                visibleTraders = allTraders.Where(t => t.UserId == currentUserId);
            }

            return View(visibleTraders);
        }

    }
}
