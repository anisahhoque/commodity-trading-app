using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace CommodityTradingApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiUrl;
        public LoginController(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
            _apiUrl = _config["api"] + "Authentication/";
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var loginData = new
            {
                Username = username,
                Password = password
            };

            
            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(loginData),
                System.Text.Encoding.UTF8,
                "application/json"
            );
            
            var response = await _httpClient.PostAsync(_apiUrl + "Login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var cookie = Request.Cookies["AuthToken"];

                TempData["Message"] = "login successful";
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ViewBag.Error = "Login failed.";
                return RedirectToAction("Login");
            }
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            var response = await _httpClient.PostAsync(_apiUrl + "Logout", null);
            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "Logged out!";
                return RedirectToAction("Login", "Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Failed to log out. Please try again.");
                return View();
            }
        }
        private bool IsUserLoggedIn()
        {
            
            return Request.Cookies.ContainsKey("AuthToken");
        }
    }
}
