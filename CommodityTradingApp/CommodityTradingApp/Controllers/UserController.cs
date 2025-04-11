using CommodityTradingApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Controllers
{
    public class UserController : Controller
    {
        // GET: User/Details/{id}
        public IActionResult Details(Guid id)
        {
            // Simulate fetching user data (replace with DB call)
            var users = new List<User>
            {
                new User { Id = Guid.Parse("e7db167e-d1b3-4b7e-b417-72a6cb85943c"), Username = "JDM", PasswordHash = "sdljfalsdjfklasdjlkaj234daljf" },
                new User { Id = Guid.Parse("1d6ffbd2-5c44-4a1e-93b2-0fc690f6e2b9"), Username = "Admin", PasswordHash = "asdfdskfj3lk4j5lkj234lkj" }
            };

            var user = users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            return View(user);
        }
    }
}
