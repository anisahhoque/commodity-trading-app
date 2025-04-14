using Microsoft.AspNetCore.Mvc;
using CommodityTradingApp.Models;
using CommodityTradingApp.ViewModels;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

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

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            return View();
        }


        [HttpPost("Trader/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditTrader model)
        {
            var updatedAccount = new
            {
                TraderId = id,
                AccountName = model.AccountName
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(updatedAccount),
                Encoding.UTF8,
                "application/json"
            );
            var response = await _httpClient.PutAsync(_apiUrl + id, jsonContent);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Trader account updated successfully.";
            }
            else
            {
                TempData["Message"] = "Failed to update trader account.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _httpClient.GetAsync(_apiUrl + id);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Trader not found.";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var trader = JsonConvert.DeserializeObject<TraderAccountPortfolioDto>(json);
            ViewBag.traderId = id;
            return View(trader);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {

            var response = await _httpClient.DeleteAsync($"{_apiUrl}{id}?confirm=true");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Trader account deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Deletion failed: {error}";
            return RedirectToAction(nameof(Delete), new { id });

         
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
