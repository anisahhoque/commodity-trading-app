using CommodityTradingApp.Models;
using CommodityTradingApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;
using System.Runtime.CompilerServices;

namespace CommodityTradingApp.Controllers
{
    public class UserController : Controller
    {
        // GET: User/Details/{guid}
        public IActionResult Details(Guid id)
        {
            // Simulate users (Replace with db call)
            var users = new List<User>
            {
                new User { Id = Guid.Parse("e7db167e-d1b3-4b7e-b417-72a6cb85943c"), Username = "JDM", PasswordHash = "sdljfalsdjfklasdjlkaj234daljf", CountryID = 1 },
                new User { Id = Guid.Parse("1d6ffbd2-5c44-4a1e-93b2-0fc690f6e2b9"), Username = "Admin", PasswordHash = "asdfdskfj3lk4j5lkj234lkj", CountryID = 2 }
            };

            var allTraders = new List<Trader>
            {
                new Trader { Id = Guid.NewGuid(), AccountName = "Alpha", Balance = 10000, UserId = users[0].Id },
                new Trader { Id = Guid.NewGuid(), AccountName = "Beta", Balance = 20000, UserId = users[0].Id },
                new Trader { Id = Guid.NewGuid(), AccountName = "Gamma", Balance = 5000, UserId = users[1].Id }
            };

            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            var userTraders = allTraders.Where(t => t.UserId == user.Id).ToList();

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
        public IActionResult Create()
        {
            return View();
        }

        //POST: User/Create
        //This is where we would hash the password and save the user to the database using BCrypt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserCreateViewModel model)
        {
            //Check if model is valid (required fields). If invalid, return same view so user can correct errors
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Hash the user's password using BCrypt for security (bcrypt creates a hashed version of the password).
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Create a new User object and populate it with data from the model.
            var user = new User
            {
                Id = Guid.NewGuid(), // Assign a new unique GUID as the user's ID.
                Username = model.Username,
                PasswordHash = hashedPassword,
                CountryID = model.CountryID
            };

            //TODO: Save the user to the database (DbContext).
            // Declare a list to simulate a "database"
            List<User> _users = new();

            // Add the newly created user to this list.
            _users.Add(user);

            // Redirect to the Details page of the newly created user.
            return RedirectToAction("Details", new { id = user.Id });
        }

        // GET: User/Edit/{guid}
        public IActionResult Edit(Guid id)
        {
            //Simulate users (Replace with db call)
            List<User> _users = new();

        var user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();  // If user not found, return a 404.
            }

            // Populate the view model with current user data.
            var model = new UserEditViewModel
            {
                Id = user.Id,
                Username = user.Username,
                CountryID = user.CountryID
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

            var user = _users.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
            {
                return NotFound();  // If user not found, return a 404.
            }

            // Update user details
            user.Username = model.Username;
            user.CountryID = model.CountryID;

            //Now, need to update the database using DbContext.

            // Redirect to the user details page after update.
            return RedirectToAction("Details", new { id = user.Id });
        }
    }
}
