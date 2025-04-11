using CommodityTradingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using CommodityTradingAPI.Models;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly CommoditiesDbContext _context;
        public UserController(CommoditiesDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        // TODO: Set up authentication to be able to handle this
        [Authorize(Roles = "Manager")]
        // Returns a json of all users
        public async Task<string> Index()
        {
            var users = await _context.Users.ToListAsync();
            return JsonConvert.SerializeObject(users);
        }

        // No need to authorise as anyone should be able to make a new account
        [HttpPost]
        [ValidateAntiForgeryToken] // Still don't really know what this does
        public async Task<IActionResult> Create([FromBody] CreateUser newUserDetails)
        {

            // Try to find the matching country
            var matchedCountry = await _context.Countries
                .FirstOrDefaultAsync(c => c.CountryName!.ToLower() == newUserDetails.Country.ToLower());

            if (matchedCountry == null)
            {
                return BadRequest(new { error = "Invalid country name." });
            }

            User newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = newUserDetails.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUserDetails.PasswordRaw),
                CountryId = matchedCountry.CountryId

            };

            _context.Add(newUser);
            await _context.SaveChangesAsync();

            var response = new UserResponse
            {
                UserId = newUser.UserId,
                Username = newUser.Username,
                CountryName = newUser.Country.CountryName!
            };

            // I will work on adding an audit log next so this code can be uncommented.
            var auditLog = new AuditLog
            {
                EntityName = "User",
                Action = "Create",
                Timestamp = DateTime.UtcNow,
                Details = $"User {newUser.Username} was created."
            };

            await _auditLogService.LogChangeAsync(auditLog);

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserId }, response);
        }

        // Get a user from ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Country)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            var response = new UserResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                CountryName = user.Country.CountryName!
            };

            return Ok(response);
        }

        public class UserResponse // DTO, basically a response object to put in the body
        {
            public Guid UserId { get; set; }
            public string Username { get; set; }
            public string CountryName { get; set; }
        }
    }
}
