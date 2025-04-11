using CommodityTradingApp.Models;
using CommodityTradingApp.ViewModels;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserCreateViewModel model)
        {

        }

    }
}
