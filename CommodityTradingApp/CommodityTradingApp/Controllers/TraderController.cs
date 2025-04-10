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


        // GET: Trader/Details/5
        public IActionResult Details(int id)
        {
            //Simulate current user and role
            bool isManager = true;
            int currentUserId = 1;

            var allTraders = new List<Trader>
        {
            new Trader { Id = Guid.NewGuid(), AccountName = "Alice", Balance = 5000, UserId = 1 },
            new Trader { Id = Guid.NewGuid(), AccountName = "Bob", Balance = 10000, UserId = 2 },
            new Trader { Id = Guid.NewGuid(), AccountName = "Charlie", Balance = 7500, UserId = 3 }
        };

            var trader = allTraders.FirstOrDefault(t => t.Id.GetHashCode() == id); // Simulate fetching trader by ID

            if (trader == null)
            {
                return NotFound();
            }

            //if not a manager, block access to other traders
            if (!isManager && trader.UserId != currentUserId)
                return Unauthorized();

            //Simulate active and historical trades
            var activeTrades = new List<Trade>
            {
                new Trade { TradeID = Guid.NewGuid(), TraderID = trader.Id, Contract = "Buy Gold", IsOpen = true}
            };
            var historicalTrades = new List<Trade>
            {
                new Trade { TradeID = Guid.NewGuid(), TraderID = trader.Id, Contract = "Sell Silver", IsOpen = false}
            };

            //Passing all of this information into the view
            var model = new TraderDetailsViewModel
            {

            }

        }
    }


}
