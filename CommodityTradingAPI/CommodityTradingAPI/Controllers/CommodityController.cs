using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly CommoditiesDbContext _context;
        private static readonly List<String> SupportedCommodities = new()
        {
            "gold"
            , "platinum", "lean_hogs", "oat", "aluminium", "soybean_meal", "lumber", "micro_gold", "feeder_cattle", "rough_rice", "palladium"
        };

        public CommodityController(IHttpClientFactory httpClientFactory, IConfiguration configuration, CommoditiesDbContext context)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Commodity>>> Index()
        {
            var availableCommodities = await _context.Commodities.ToListAsync();
            return availableCommodities;
        }

        [HttpGet("{commodityName}")]
        public async Task<IActionResult> GetCommodityPrice(string commodityName)
        {
            if (!SupportedCommodities.Contains(commodityName.ToLower()))
            {
                return BadRequest("Unsupported commodity");
            }

            var apiKey = _configuration["X-Api-Key"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, "API key not configured");
            }

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.api-ninjas.com/v1/commodityprice?name={commodityName}");


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

        [HttpGet("{commodityName}/{timePeriod}/{startTime}/{endTime}")]
        public async Task<IActionResult> GetCommodityPriceHistorical(string commodityName, string timePeriod, string startTime, string endTime)
        {
            if (!SupportedCommodities.Contains(commodityName.ToLower()))
            {
                return BadRequest("Unsupported commodity");
            }

            var apiKey = _configuration["X-Api-Key"];
            if (string.IsNullOrEmpty(apiKey))
            {
                return StatusCode(500, "API key not configured");
            }

            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.api-ninjas.com/v1/commoditypricehistorical?name={commodityName}&period={timePeriod}&start={startTime}&end={endTime}");


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

        [HttpGet("Details/{cId}")]
        public async Task<IActionResult> CommodityDetails(Guid cId)
        {
            var commod = await _context.Commodities.FirstOrDefaultAsync(x => x.CommodityId == cId);

            if (commod == null)
                return NotFound();

            return Ok(commod);
        }


        
    }
}