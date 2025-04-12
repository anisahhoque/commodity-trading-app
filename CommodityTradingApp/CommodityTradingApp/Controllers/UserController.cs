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
        // GET: User/Details/{guid}
        private static List<User> users =
         new List<User>
            {
                new User { UserId = Guid.Parse("e7db167e-d1b3-4b7e-b417-72a6cb85943c"), Username = "JDM", PasswordHash = "sdljfalsdjfklasdjlkaj234daljf", CountryId = 1 },
                new User { UserId = Guid.Parse("1d6ffbd2-5c44-4a1e-93b2-0fc690f6e2b9"), Username = "Admin", PasswordHash = "asdfdskfj3lk4j5lkj234lkj", CountryId = 2 }
            };

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
        public IActionResult Details(Guid id)
        {
            // Simulate users (Replace with db call)


            var allTraders = new List<Trader>
            {
                new Trader { Id = Guid.NewGuid(), AccountName = "Alpha", Balance = 10000, UserId = users[0].UserId },
                new Trader { Id = Guid.NewGuid(), AccountName = "Beta", Balance = 20000, UserId = users[0].UserId },
                new Trader { Id = Guid.NewGuid(), AccountName = "Gamma", Balance = 5000, UserId = users[1].UserId }
            };

            var user = users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
                return NotFound();

            var userTraders = allTraders.Where(t => t.UserId == user.UserId).ToList();

            //Passing the user and their traders to the view
            var viewModel = new UserDetailsViewModel
            {
                User = user,
                Traders = userTraders
            };

            return View(viewModel);
        }

        //Creating a user: Potentially have this as a button in login page???
        //GET: User/Create
        [HttpGet]
        public async Task< IActionResult> Create()//UserCreateViewModel ucvM)
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

        //POST: User/Create
        //This is where we would hash the password and save the user to the database using BCrypt
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

        // GET: User/Edit/{guid}
        public IActionResult Edit(Guid id)
        {
            //Simulate users (Replace with db call)
            List<User> _users = new();

        var user = _users.FirstOrDefault(u => u.UserId == id);
            if (user == null)
            {
                return NotFound();  // If user not found, return a 404.
            }

            // Populate the view model with current user data.
            var model = new UserEditViewModel
            {
                Id = user.UserId,
                Username = user.Username,
                CountryId = user.CountryId
            };

            return View(model);  // Return the edit view with the model data.
        }

        // POST: User/Edit/{guid}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);  // If validation fails, return the same view with validation errors.
            }

            // Simulate users (Replace with db call)
            List<User> _users = new();

            var user = _users.FirstOrDefault(u => u.UserId == model.Id);
            if (user == null)
            {
                return NotFound();  // If user not found, return a 404.
            }

            // Update user details
            user.Username = model.Username;
            user.CountryId = model.CountryId;

            //Now, need to update the database using DbContext.

            // Redirect to the user details page after update.
            return RedirectToAction("Details", "User/"+new { id = user.UserId });
        }

        // GET: User/Delete/{guid} (Shows confirmation view)
        public IActionResult Delete(Guid userId)
        {
            // Simulate users (Replace with db call)
            List<User> _users = new();

            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();  // User not found
            }

            // Return confirmation view
            return View(user);
        }

        // POST: User/Delete/5 (Perform the delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid userId)
        {
            bool isManager = true;  // This should be checked from session or user role

            // Simulate users (Replace with db call)
            List<User> _users = new();

            if (!isManager)
            {
                return Unauthorized();  // Return unauthorized if not a manager
            }

            var user = _users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound();  // User not found
            }

            _users.Remove(user);  // Delete the user

            // Redirect to a success page or the user list
            TempData["Message"] = "User deleted successfully!";
            return RedirectToAction("Index");  // Assuming Index shows the list of users
        }
    }
}
