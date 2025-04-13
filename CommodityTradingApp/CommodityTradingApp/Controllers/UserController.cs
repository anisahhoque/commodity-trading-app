using CommodityTradingApp.Models;
using CommodityTradingApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace CommodityTradingApp.Controllers
{
    public class UserController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiUrl;
        private readonly string _apiUrlCountry;
        private readonly string _apiUrlRole;
        public UserController(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
            _apiUrl = _config["api"] + "User/";
            _apiUrlCountry = _config["api"] + "Country/";
            _apiUrlRole = _config["api"] + "Role";

        }


        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(json);
                return View(users);
            }
            else
            {
                
                ModelState.AddModelError(string.Empty, "Unable to retrieve users from the API");
                return View(new List<User>());
            }
        }
      
        public async Task<IActionResult> Details(Guid id)
        
        {

            var response = await _httpClient.GetAsync(_apiUrl + id);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(json);
                return View(user);
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Unable to retrieve user from the API");
                return View(new User());
            }
        

            
        }


        [HttpGet]
        public async Task< IActionResult> Create()
        {
            var response = await _httpClient.GetAsync(_apiUrlCountry);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var countries = JsonConvert.DeserializeObject<List<Country>>(json);

                
                ViewBag.Countries = new SelectList(countries, "CountryName", "CountryName");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            var responseRole = await _httpClient.GetAsync(_apiUrlRole);
            if (responseRole.IsSuccessStatusCode)
            {
                var json = await responseRole.Content.ReadAsStringAsync();
                var roles = JsonConvert.DeserializeObject<List<Role>>(json);


                ViewBag.Roles = new SelectList(roles, "RoleName", "RoleName");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
            

        }


        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string password, string country, string role)
        {

            //Console.WriteLine("country:" + country);

            var userData = new
            {
                Username = username,
                PasswordRaw = password,
                Country = country,
                Role = role
            };
            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(userData),
                System.Text.Encoding.UTF8,
                "application/json"
            );



            var response = await _httpClient.PostAsync(_apiUrl + "Create", jsonContent);




            if (response.IsSuccessStatusCode)
            {
                TempData["Message"] = "New User Created!";
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Could not create user");
                return RedirectToAction("Index", "Home");
            }

            return RedirectToAction("Login", "Login");
        }

   
        public IActionResult Edit(Guid id)
        {

            return View();  
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit()
        {

            return RedirectToAction("Details", "User");
        }

        public IActionResult Delete(Guid userId)
        {

            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid userId)
        {

            TempData["Message"] = "User deleted successfully!";
            return RedirectToAction("Index"); 
        }


    }
}
