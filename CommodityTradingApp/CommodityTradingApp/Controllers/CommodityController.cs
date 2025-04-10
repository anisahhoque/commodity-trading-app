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
            return View(commodities);
        }
        private static List<Commodity> GetMockCommodities()
        {
            return new List<Commodity>
            {
                new Commodity
                {
                    CommodityId = Guid.NewGuid(),
                    CommodityName = "Gold"
                },
                new Commodity
                {
                    CommodityId = Guid.NewGuid(),
                    CommodityName = "Oil"

                },
                new Commodity
                {
                    CommodityId = Guid.NewGuid(),
                    CommodityName = "Silver"

                }
            };
        }
    }
}
