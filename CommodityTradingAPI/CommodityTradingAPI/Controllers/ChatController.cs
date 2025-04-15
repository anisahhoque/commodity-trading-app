using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using CommodityTradingAPI.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
    
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly string _chatUrl;
        private readonly string _adminUserId;
        private readonly string _adminAuthToken;
        public ChatController(IConfiguration configuration, HttpClient httpClient)
        {
     
            _configuration = configuration;
            _httpClient = httpClient;
            _chatUrl = _configuration["rocket-chat-url"];
            _adminUserId = _configuration["userId"];
            _adminAuthToken = _configuration["authToken"];
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginToChat([FromBody] ChatLoginDto model)//[Bind("Username", "Password", "Name", "Email")] ChatLoginDto model)
        {
            var username = model.Username;
            var password = model.Password;
            var email = model.Email;
            var name = model.Name;

            _httpClient.DefaultRequestHeaders.Add("X-Auth-Token", _adminAuthToken);
            _httpClient.DefaultRequestHeaders.Add("X-User-Id", _adminUserId);
            var userCreateResponse = await _httpClient.PostAsync(
                $"{_chatUrl}/api/v1/users.create",
                new StringContent(JsonConvert.SerializeObject(new
                {
                    username = username,
                    name = name,
                    email = email,
                    password = password
                }), Encoding.UTF8, "application/json")
            );

            
            var loginResponse = await _httpClient.PostAsync(
                $"{_chatUrl}/api/v1/login",
                new StringContent(JsonConvert.SerializeObject(new
                {
                    username = username,
                    password = password
                }), Encoding.UTF8, "application/json")
            );

            if (!loginResponse.IsSuccessStatusCode)
                return Unauthorized();

            var json = await loginResponse.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(json);

            return Ok(new
            {
                Token = (string)data.data.authToken,
                UserId = (string)data.data.userId
            });
        }

    }
}
