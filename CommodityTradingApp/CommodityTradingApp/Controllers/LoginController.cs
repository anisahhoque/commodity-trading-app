using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommodityTradingApp.Models;


namespace CommodityTradingApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiUrl;
        public LoginController(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
            _apiUrl = _config["api"] + "Authentication/";
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var loginData = new
            {
                Username = username,
                Password = password
            };


            var jsonContent = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(loginData),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            //var response = await _httpClient.PostAsync(_apiUrl + "Login", jsonContent);

            //if (response.IsSuccessStatusCode)
            //{
            //    var cookie = Request.Cookies["AuthToken"];

            //    TempData["Message"] = "login successful";
            //    return RedirectToAction("Login", "Login");
            //}
            //else
            //{
            //    ViewBag.Error = "Login failed.";
            //    return RedirectToAction("Login");
            //}

            var response = await _httpClient.PostAsync(_apiUrl + "Login", jsonContent);
            User user = new User
            {
                Username = username,
                PasswordHash = password
            };
            if (response.IsSuccessStatusCode)
            {
                var token = CreateJwt(user);
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                    Secure = true,
                    SameSite = SameSiteMode.None
                };
                Response.Cookies.Append("AuthToken", token, cookieOptions);
                TempData["Message"] = "Login successful!";
            } else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }
            return RedirectToAction("Login", "Login");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            //var response = await _httpClient.PostAsync(_apiUrl + "Logout", null);
            //if (response.IsSuccessStatusCode)
            //{
            //    TempData["Message"] = "Logged out!";
            //    return RedirectToAction("Login", "Login");
            //}
            //else
            //{
            //    ModelState.AddModelError(string.Empty, "Failed to log out. Please try again.");
            //    return View();
            //}
            Response.Cookies.Delete("AuthToken");
            TempData["Message"] = "Logged out!";
            return RedirectToAction("Login", "Login");
        }

        private string CreateJwt(User user)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(5),
                SigningCredentials = creds,
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            return jwt;
        }
    }
}
