using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommodityController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private static readonly List<string> SupportedCommodities = new List<string>
        {
            "gold"
            //, "platinum", "lean_hogs", "oat", "aluminium", "soybean_meal", "lumber", "micro_gold", "feeder_cattle", "rough_rice", "palladium"
        };

        public CommodityController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetCommodityList()
        {
            return Ok(SupportedCommodities);
        }

        [HttpGet("Commodity/{commodityName}")]
        public async Task<IActionResult> GetCommodityPrice(string commodityName)
        {
            if (!SupportedCommodities.Contains(commodityName.ToLower()))
            {
                return BadRequest("Unsupported commodity");
            }

            var apiKey = _configuration["X-Api-Key"];

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.api-ninjas.com/v1/commodities?name={commodityName}");

            request.Headers.Add("X-Api-Key", apiKey);

            try
            {
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Ok(content);
                }

                return StatusCode(
                    (int)response.StatusCode,
                    $"Error retrieving data from API. Status: {response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error connecting to API: {ex.Message}");
            }
        }
    }
}