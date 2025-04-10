using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using Microsoft.AspNetCore.Mvc;
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
            var token = CreateJwt(dbUser);

            Response.Cookies.Append("AuthToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(5)
            });

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AuthToken");
            return Ok(new { message = "Logged out successfully" });
        }

        private string CreateJwt(User user)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = creds,
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            return jwt;
        }
    }
}
