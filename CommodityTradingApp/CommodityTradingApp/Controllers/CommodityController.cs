using CommodityTradingApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Controllers
{
    public class CommodityController : Controller

    {
        private readonly HttpClient _httpClient;
        private static List<Commodity> commodities = GetMockCommodities();

        public CommodityController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<IActionResult> Index()
        {
            //call get list of commodities
            return View(commodities);
        }
        private static List<Commodity> GetMockCommodities()
        {
            return new List<Commodity>
            {
                new Commodity
                {
                    CommodityId = Guid.Parse("7c472314-bb42-4e02-86c1-7fc28e6de3b6"),
                    CommodityName = "Gold"
                    
                },
                new Commodity
                {
                    CommodityId = Guid.Parse("92d2573e-3ad1-4f38-8859-9d4d9e9e3a0e"),
                    CommodityName = "Oil"

                },
                new Commodity
                {
                    CommodityId = Guid.Parse("f8c3618a-53a8-4097-871f-50a924bdcf1c") ,
                    CommodityName = "Silver"

                }
            };
        }
    }
}
