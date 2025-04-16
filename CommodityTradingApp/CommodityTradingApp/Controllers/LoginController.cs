using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CommodityTradingApp.Models;
using System.Text.Json;
using CommodityTradingApp.Models.DTOs;


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
        public async Task<IActionResult> Login(string username, string password, string email)
        {
            var loginData = new
            {
                Username = username,
                Password = password
            };



            var jsonContent = new StringContent(
                JsonSerializer.Serialize(loginData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(_apiUrl + "Login", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                var user = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Login failed: user not found.");
                    return View();
                }

                var token = await CreateJwtAsync(user);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                };

                Response.Cookies.Append("AuthToken", token, cookieOptions);
                //TempData["Message"] = "Login successful!";

                var chatLoginDto = new ChatLoginDto
                {
                    Username = username,
                    Password = password,
                    Name = username,
                    Email = email
                };
                var chatLoginContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(chatLoginDto),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );
                var chatLoginResponse = await _httpClient.PostAsync(_config["api"] + "Chat/login", chatLoginContent);

                if (chatLoginResponse.IsSuccessStatusCode)
                {
                    var chatResponseContent = await chatLoginResponse.Content.ReadAsStringAsync();

                    var chatLoginResult = JsonSerializer.Deserialize<RocketChatLoginResponse>(chatResponseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (chatLoginResult != null)
                    {
                        
                        HttpContext.Session.SetString("RocketChatToken", chatLoginResult.Token);
                        HttpContext.Session.SetString("RocketChatUserId", chatLoginResult.UserId);

                        
                        TempData["Message"] = "Chat login successful!";
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Rocket.Chat login failed.");
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("RocketChatToken");
            HttpContext.Session.Remove("RocketChatUserId");
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Cookies.Delete("AuthToken");
                TempData["Message"] = "Logged out!";
            }
            else
            {
                TempData["Message"] = "No active session found.";
            }
            
            return RedirectToAction("Login", "Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task<string> CreateJwtAsync(LoginResponseDto user)
        {
            var keyFromConfig = _config["JwtSettings:Key"];
            var userID = user.UserId;
            string fullUrl = _apiUrl + "role/" + userID.ToString();
            var roleResponse = await _httpClient.GetAsync(fullUrl);

            string role = null;
            if (roleResponse.IsSuccessStatusCode)
            {
                role = await roleResponse.Content.ReadAsStringAsync();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            };

            if (!string.IsNullOrEmpty(role))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var keyBytes = Encoding.UTF8.GetBytes(keyFromConfig);
            var key = new SymmetricSecurityKey(keyBytes);

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            Console.WriteLine($"Key byte length: {keyBytes.Length}");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
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
