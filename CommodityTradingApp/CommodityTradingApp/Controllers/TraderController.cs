using Microsoft.AspNetCore.Mvc;
using CommodityTradingApp.Models;
using CommodityTradingApp.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using Microsoft.AspNetCore.Authorization;


namespace CommodityTradingApp.Controllers
{
    [Authorize]
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
                ViewBag.traderId = id;
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



            var response = await _httpClient.PostAsync(_apiUrl , jsonContent);




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



            return View();
        }

        // POST: Trader/Edit/c9b9f2c5-4d95-4b6a-bb6a-dc3d70a5d8f4
        [HttpPost]
        public IActionResult Edit(EditTraderViewModel model)
        {

            return RedirectToAction("Index");
        }

        // GET: Trader/Delete/{guid}
        //This is the view for deleting a trader
        public IActionResult Delete(Guid id)
        {


            return View(); // Confirm delete
        }


        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            


            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(Guid id, decimal amount)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("", "Deposit amount must be greater than zero.");
                return RedirectToAction("Details", new { id });

            }

            var depositData = new
            {
                TraderId = id,
                Amount = amount
            };

            
            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(depositData),
                Encoding.UTF8,
                "application/json"
            );

            
            var response = await _httpClient.PostAsync(_apiUrl + "Deposit/" + id, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                
                TempData["Message"] = "Deposit successful!";
                return RedirectToAction("Details", new { id });

            }
            else
            {
                
                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Error: {errorMessage}");
                return RedirectToAction("Details", new { id });
            }
        }


        public async Task<IActionResult> Withdraw(Guid id, decimal amount)
        {

            if (amount <= 0)
            {
                ModelState.AddModelError("", "Withdraw amount must be greater than zero.");
                return RedirectToAction("Details", new { id });

            }

            var depositData = new
            {
                TraderId = id,
                Amount = amount
            };


            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(depositData),
                Encoding.UTF8,
                "application/json"
            );


            var response = await _httpClient.PostAsync(_apiUrl + "Withdraw/" + id, jsonContent);

            if (response.IsSuccessStatusCode)
            {

                TempData["Message"] = "Withdrawal successful!";
                return RedirectToAction("Details", new { id });

            }
            else
            {

                var errorMessage = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Error: {errorMessage}");
                return RedirectToAction("Details", new { id });
            }
        }


       
 

    }
}
