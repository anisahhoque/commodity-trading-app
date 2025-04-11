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
        private static List<User> users =
         new List<User>
            {
                new User { UserId = Guid.Parse("e7db167e-d1b3-4b7e-b417-72a6cb85943c"), Username = "JDM", PasswordHash = "sdljfalsdjfklasdjlkaj234daljf", CountryId = 1 },
                new User { UserId = Guid.Parse("1d6ffbd2-5c44-4a1e-93b2-0fc690f6e2b9"), Username = "Admin", PasswordHash = "asdfdskfj3lk4j5lkj234lkj", CountryId = 2 }
            };
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
        public IActionResult Create()
        {
            return View(new UserCreateViewModel { Username = "", Password = "", CountryId = 1 });
            
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
                UserId = Guid.NewGuid(), // Assign a new unique GUID as the user's ID.
                Username = model.Username,
                PasswordHash = hashedPassword,
                CountryId = (byte)model.CountryId
            };

            //TODO: Save the user to the database (DbContext).
            // Declare a list to simulate a "database"
            //List<User> _users = new();

            // Add the newly created user to this list.
            users.Add(user);

            // Redirect to the Details page of the newly created user.
            return RedirectToAction("Details", new { id = user.UserId });
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
