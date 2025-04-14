using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using CommodityTradingAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly CommoditiesDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationController(CommoditiesDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }


        [HttpPost("Login")]
        public ActionResult<User> Login([Bind("Username,Password")] LoginRequest request)
        {
            var dbUser = _context.Users.FirstOrDefault(u => u.Username == request.Username);
            if (dbUser == null || !BCrypt.Net.BCrypt.Verify(request.Password, dbUser.PasswordHash))
            {
                return BadRequest("Invalid login");
            }

            return Ok();
        }

        [HttpGet("role/{userId}")]
        public  ActionResult<string> UserRole(Guid userId)
        {
            var user =  _context.Users
                                  .Include(u => u.RoleAssignments)
                                  .ThenInclude(ra => ra.Role)
                                  .FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get the role of the user
            var role = user.RoleAssignments.FirstOrDefault()?.Role?.RoleName;

            if (role == null)
            {
                return NotFound("Role not found");
            }

            return Ok(role);
        }

    }
}
