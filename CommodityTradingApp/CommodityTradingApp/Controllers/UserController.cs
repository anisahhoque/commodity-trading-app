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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string username, string password, string country, string role)
        {


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


        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<EditUser>(json);

     
            user.AllCountries = await GetCountries();
            user.AllRoles = await GetRoles();

            return View(user);
        }



        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditUser model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";
                model.AllCountries = await GetCountries(); 
                model.AllRoles = await GetRoles();
                return View(model);
            }

            
            var updateUser = new
            {
                UserId = model.UserId,
                Password = model.Password, 
                CountryId = model.CountryId,
                SelectedRoleIds = model.SelectedRoleIds
            };

            var content = new StringContent(JsonConvert.SerializeObject(updateUser), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiUrl}{model.UserId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Update failed: {error}";
                model.AllCountries = await GetCountries(); 
                model.AllRoles = await GetRoles();
                return View(model);
            }
        }



        [HttpGet("Delete/{id}")]

        public async Task<IActionResult> Delete(Guid id)
        {
            var response = await _httpClient.GetAsync(_apiUrl + id);
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<User>(json);

            return View(user);
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}{id}?confirm=true");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "User deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["Error"] = $"Deletion failed: {error}";
            return RedirectToAction(nameof(Delete), new { id });
        }
        private async Task<IEnumerable<SelectListItem>> GetCountries()
        {
            var response = await _httpClient.GetAsync(_apiUrlCountry);
            var countriesJson = await response.Content.ReadAsStringAsync();
            var countries = JsonConvert.DeserializeObject<List<Country>>(countriesJson);
            return countries.Select(c => new SelectListItem
            {
                Value = c.CountryId.ToString(),
                Text = c.CountryName
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetRoles()
        {
            var response = await _httpClient.GetAsync(_apiUrlRole);
            var rolesJson = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<Role>>(rolesJson);
            return roles.Select(r => new SelectListItem
            {
                Value = r.RoleId.ToString(),
                Text = r.RoleName
            });
        }


    }
}
