using Microsoft.AspNetCore.Mvc;
using CommodityTradingApp.Models;
using CommodityTradingApp.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CommodityTradingApp.Controllers
{


    public class TraderController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiUrl;
        public TraderController(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
            _apiUrl = _config["api"] + "Trader/";

        }
        public async Task<IActionResult> Index()
        {
            var response = _httpClient.GetAsync(_apiUrl).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var traders = JsonConvert.DeserializeObject<List<TraderAccount>>(json);
                return View(traders);
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Unable to retrieve traders from the API");
                return View(new List<TraderAccount>());
            }

           
        }


        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var response = _httpClient.GetAsync(_apiUrl + id).Result;
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var trader = JsonConvert.DeserializeObject<TraderAccountPortfolioDto>(json);
                return View(trader);
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Unable to retrieve trader account from the API");
                return View(new TraderAccountPortfolioDto());
            }
           
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var response = await _httpClient.GetAsync(_config["api"] + "User/");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(json);
                ViewBag.Users = new SelectList(users, "UserId", "Username");
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Unable to retrieve users from the API");
                return View(new List<User>());
            }
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Guid userid,string accountname,decimal balance)
        {
            var userData = new
            {
                UserId = userid,
                Balance = balance,
                AccountName = accountname
            };
                
            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(userData),
                System.Text.Encoding.UTF8,
                "application/json"
            );



            var response = await _httpClient.PostAsync(_apiUrl + "Create", jsonContent);




            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "New Trader Account Created!";
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Could not create trader account");
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Index");

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

        // GET: Trader/Deposit/{guid}
        public IActionResult Deposit(Guid id)
        {
            // Simulated data fetch - replace with DB context
            var trader = new Trader
            {
                Id = id,
                AccountName = "Alice",
                Balance = 100000,
                UserId = Guid.NewGuid()
            };

            if (trader == null)
                return NotFound();

            return View(trader); // Show deposit form
        }

        // POST: Trader/Deposit/{guid}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Deposit(Guid id, decimal depositAmount)
        {
            // Simulated data fetch - replace with DB context
            var trader = new Trader
            {
                Id = id,
                AccountName = "Alice",
                Balance = 100000,
                UserId = Guid.NewGuid()
            };

            if (trader == null)
                return NotFound();

            // Update balance
            trader.Balance += depositAmount;

            // In real app, save changes to DB here

            TempData["SuccessMessage"] = $"Successfully deposited {depositAmount:C} into {trader.AccountName}'s account.";
            return RedirectToAction("Details", new { id = trader.Id });
        }

        // GET: Trader/Withdraw/{guid}
        public IActionResult Withdraw(Guid id)
        {
            bool isManager = true;
            if (!isManager)
                return Unauthorized();

            // Simulate finding trader (replace with DB logic)
            var trader = new Trader
            {
                Id = id,
                AccountName = "Bob",
                Balance = 100000,
                UserId = Guid.NewGuid()
            };

            if (trader == null)
                return NotFound();

            return View(trader);
        }


        // POST: Trader/Withdraw/{guid}
        [HttpPost]
        public IActionResult Withdraw(Guid id, decimal withdrawAmount)
        {
            // Simulate finding trader
            var trader = new Trader
            {
                Id = id,
                AccountName = "Bob",
                Balance = 100000,
                UserId = Guid.NewGuid()
            };

            if (trader == null)
                return NotFound();

            if (withdrawAmount <= 0)
            {
                ModelState.AddModelError("", "Withdraw amount must be greater than zero.");
                return View(trader);
            }

            if (withdrawAmount > trader.Balance)
            {
                ModelState.AddModelError("", "Insufficient balance.");
                return View(trader);
            }

            // Apply withdrawal
            trader.Balance -= withdrawAmount;

            // Simulate DB save
            Console.WriteLine($"Updated Trader {trader.AccountName}'s balance: {trader.Balance}");

            return RedirectToAction("Index");
        }

    }
}
