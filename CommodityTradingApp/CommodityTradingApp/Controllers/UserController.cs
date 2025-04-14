using CommodityTradingApp.Models;
using CommodityTradingApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Runtime.CompilerServices;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CommodityTradingApp.Controllers
{
    [Authorize]
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
            //var response = await _httpClient.GetAsync(_apiUrl);
            //if (response.IsSuccessStatusCode)
            //{
            //    var json = await response.Content.ReadAsStringAsync();
            //    var users = JsonConvert.DeserializeObject<List<User>>(json);
            //    return View(users);
            //}
            //else
            //{

            //    ModelState.AddModelError(string.Empty, "Unable to retrieve users from the API");
            //    return View(new List<User>());
            //}

            var token = Request.Cookies["AuthToken"];
            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Login");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;


            if (role == "Manager")
            {
                var response = await _httpClient.GetAsync(_apiUrl);
                var allUsers = await response.Content.ReadFromJsonAsync<List<User>>();
                return View(allUsers);
            }
            else
            {
                var response = await _httpClient.GetAsync(_apiUrl + userId);
                var individualUser = await response.Content.ReadFromJsonAsync<User>();
                return View(new List<User> { individualUser });
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


        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                
                var userResponse = await _httpClient.GetAsync($"{_apiUrl}{id}");
                userResponse.EnsureSuccessStatusCode();

                var userContent = await userResponse.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(userContent);

                if (user == null)
                {
                    return NotFound();
                }

                
                var editUser = new EditUser
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    CountryId = user.CountryId,
                    CurrentCountryName = user.Country?.CountryName,
                    SelectedRoleIds = user.RoleAssignments.Select(ra => ra.RoleId).ToList()
                };

                
                var countriesResponse = await _httpClient.GetAsync(_apiUrlCountry);
                countriesResponse.EnsureSuccessStatusCode();

                var countriesContent = await countriesResponse.Content.ReadAsStringAsync();
                var countries = JsonConvert.DeserializeObject<List<Country>>(countriesContent);


                editUser.AllCountries = countries;

               
                var rolesResponse = await _httpClient.GetAsync(_apiUrlRole);
                rolesResponse.EnsureSuccessStatusCode();

                var rolesContent = await rolesResponse.Content.ReadAsStringAsync();
                var roles = JsonConvert.DeserializeObject<List<Role>>(rolesContent);

                
                var userRoleIds = user.RoleAssignments.Select(ra => ra.RoleId).ToList();
                editUser.AllRoles = roles;
                return View(editUser);
            }
            catch (Exception ex)
            {
              
                TempData["ErrorMessage"] = $"Failed to retrieve user data: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("User/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, EditUser model)
        {
            var rolesResponse = await _httpClient.GetAsync(_apiUrlRole);
            rolesResponse.EnsureSuccessStatusCode();

            var rolesContent = await rolesResponse.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<Role>>(rolesContent);
            var countriesResponse = await _httpClient.GetAsync(_apiUrlCountry);
            countriesResponse.EnsureSuccessStatusCode();

            var countriesContent = await countriesResponse.Content.ReadAsStringAsync();
            var countries = JsonConvert.DeserializeObject<List<Country>>(countriesContent);
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input.";

                model.AllCountries = countries;

                model.AllRoles = roles;
                return View(model);
            }

            
            var updateUser = new
            {
                UserId = id,
                Username = model.Username,
                Password = model.Password, 
                CountryId = model.CountryId,
                SelectedRoleIds = model.SelectedRoleIds
            };

            var content = new StringContent(JsonConvert.SerializeObject(updateUser), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"{_apiUrl}{id}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "User updated successfully.";

                return RedirectToAction(nameof(Index));

            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"Update failed: {error}";
                model.AllCountries = countries;
                model.AllRoles = roles;
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
