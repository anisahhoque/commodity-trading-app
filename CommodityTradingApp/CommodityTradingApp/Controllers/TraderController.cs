using Microsoft.AspNetCore.Mvc;
using CommodityTradingApp.Models;
using CommodityTradingApp.ViewModels;

namespace CommodityTradingApp.Controllers
{
    //Need to ensure i have DBContext, and inject it into controller
    //DBcontext must have class that represents DbSet<Trader> Traders


    public class TraderController : Controller
    {
        // GET: Trader/Index
        public IActionResult Index()
        {
            //TODO: DELETE WHEN CONNECTED TO DB
            // Simulate current user and role
            bool isManager = true; // Change to false to test user view
            Guid currentUserId = Guid.Parse("a67c1bde-32ff-4c60-89ef-90b7dbd45f2e");

            var allTraders = new List<Trader>
    {
        new Trader { Id = Guid.NewGuid(), AccountName = "Alice", Balance = 5000, UserId = Guid.NewGuid() },
        new Trader { Id = Guid.NewGuid(), AccountName = "Bob", Balance = 10000, UserId = Guid.NewGuid() },
        new Trader { Id = Guid.NewGuid(), AccountName = "Charlie", Balance = 7500, UserId = Guid.NewGuid() }
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


        // GET: Trader/Details/c9b9f2c5-4d95-4b6a-bb6a-dc3d70a5d8f4
        public IActionResult Details(Guid id)
        {
            //TODO: DELETE WHEN CONNECTED TO DB
            //Simulate current user and role
            bool isManager = true;
            Guid currentUserId = Guid.Parse("a67c1bde-32ff-4c60-89ef-90b7dbd45f2e");

            var allTraders = new List<Trader>
        {
            new Trader { Id = Guid.Parse("c9b9f2c5-4d95-4b6a-bb6a-dc3d70a5d8f4"), AccountName = "Alice", Balance = 5000, UserId = Guid.NewGuid() },
            new Trader { Id = Guid.Parse("a9b7e68d-6b16-4f8d-ae88-9b64a9392e44"), AccountName = "Bob", Balance = 10000, UserId = Guid.NewGuid() },
            new Trader { Id = Guid.Parse("d2db8f71-e6a1-4f3b-b1b3-b8d50b7cb327"), AccountName = "Charlie", Balance = 7500, UserId = Guid.NewGuid()}
        };

            var trader = allTraders.FirstOrDefault(t => t.Id == id); // Simulate fetching trader by ID

            if (trader == null)
            {
                return NotFound();
            }

            //if not a manager, block access to other traders
            if (!isManager && trader.UserId != currentUserId)
                return Unauthorized();

            //TODO: DELETE WHEN CONNECTED TO DB
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
                Trader = trader,
                ActiveTrades = activeTrades,
                HistoricalTrades = historicalTrades
            };

            return View(model);
        }

        // GET: Trader/Create
        //This is the view for creating a new trader
        public IActionResult Create()
        {
            return View();
        }

        // POST: Trader/Create
        //This is the action that handles the form submission for creating a new trader
        [HttpPost]
        public IActionResult Create(CreateTraderViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newTrader = new Trader
                {
                    Id = Guid.NewGuid(),
                    AccountName = model.AccountName,
                    Balance = model.Balance,
                    UserId = model.UserId
                };

                return RedirectToAction("Index");
            }

            return View(model);
        }

        // GET: Trader/Edit/c9b9f2c5-4d95-4b6a-bb6a-dc3d70a5d8f4
        //This is the view for editing a trader
        public IActionResult Edit(Guid id)
        {
            // Simulated data (replace with DB context query in real app)
            var trader = new Trader
            {
                Id = id,
                AccountName = "Alice",
                Balance = 5000,
                UserId = Guid.NewGuid()
            };

            if (trader == null)
            {
                return NotFound();
            }

            var viewModel = new EditTraderViewModel
            {
                Id = trader.Id,
                AccountName = trader.AccountName,
                Balance = trader.Balance
            };

            return View(viewModel);
        }

        // POST: Trader/Edit/c9b9f2c5-4d95-4b6a-bb6a-dc3d70a5d8f4
        [HttpPost]
        public IActionResult Edit(EditTraderViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // In real app: fetch trader from DB by ID and update fields
            // Simulate update success
            Console.WriteLine($"Updated Trader {model.Id}: Name={model.AccountName}, Balance={model.Balance}");

            return RedirectToAction("Index");
        }

        // GET: Trader/Delete/{guid}
        //This is the view for deleting a trader
        public IActionResult Delete(Guid id)
        {
            // Simulated manager check
            bool isManager = true;

            if (!isManager)
                return Unauthorized();

            // Simulated data (replace with DB call in real implementation)
            var trader = new Trader
            {
                Id = id,
                AccountName = "Bob",
                Balance = 10000,
                UserId = Guid.NewGuid()
            };

            if (trader == null)
                return NotFound();

            return View(trader); // Confirm delete
        }

        // POST: Trader/Delete/{guid}
        //This is the action that handles the form submission for deleting a trader
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            // Simulated manager check
            bool isManager = true;

            if (!isManager)
                return Unauthorized();

            // Here you'd remove the trader from DB
            Console.WriteLine($"Deleted Trader with ID: {id}");

            return RedirectToAction("Index");
        }
    }
}
