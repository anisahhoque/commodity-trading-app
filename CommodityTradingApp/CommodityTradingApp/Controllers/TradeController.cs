using CommodityTradingApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Controllers
{
    public class TradeController : Controller
    {
        private readonly HttpClient _httpClient;
        private static List<Trade> trades = GetMockTrades();
        private static List<Commodity> commodities = GetMockCommodities();
        public TradeController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<IActionResult> Index()
        {
            return View(trades);
        }
        public async Task<IActionResult> TradeHistory(int page = 1, int pageSize = 10,bool? isOpen = false)
        {
            var filteredTrades = isOpen.HasValue ? trades.Where(t => t.IsOpen == isOpen.Value).ToList() : trades;
            var totalTrades = filteredTrades.Count();
            var pagedTrades = filteredTrades.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            
            

            return View(pagedTrades);
        }

        public async Task<IActionResult> ActiveTrades(int page = 1, int pageSize = 10, bool? isOpen = true)
        {
            var filteredTrades = isOpen.HasValue ? trades.Where(t => t.IsOpen == isOpen.Value).ToList() : trades;
            var totalTrades = filteredTrades.Count();
            var pagedTrades = filteredTrades.Skip((page - 1) * pageSize).Take(pageSize).ToList();




            return View(pagedTrades);
        }
        public async Task<IActionResult> CreateTrade()
        {
            //call get details for each commodity in commodities
            return View(commodities);
        }
        public async Task<IActionResult> DeleteTrade(Guid TradeId)
        {
            //
            return RedirectToAction("Index");
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
        private static List<Trade> GetMockTrades()
        {
            return new List<Trade>
            {
                new Trade
                {
                    TradeId = Guid.NewGuid(),
                    TraderId = Guid.NewGuid(),
                    CommodityId = Guid.NewGuid(),
                    PricePerUnit = 150.75m,
                    Quantity = 100,
                    IsBuy = true,
                    Expiry = DateTime.Now.AddDays(30),
                    CreatedAt = DateTime.Now,
                    Bourse = "NYSE",
                    MitigationId = Guid.NewGuid(),
                    IsOpen = true,
                    Contract = "XYZ123"
                },
                new Trade
                {
                    TradeId = Guid.NewGuid(),
                    TraderId = Guid.NewGuid(),
                    CommodityId = Guid.NewGuid(),
                    PricePerUnit = 120.50m,
                    Quantity = 50,
                    IsBuy = false,
                    Expiry = DateTime.Now.AddDays(60),
                    CreatedAt = DateTime.Now,
                    Bourse = "LSE",
                    MitigationId = Guid.NewGuid(),
                    IsOpen = false,
                    Contract = "ABC456"
                },
                new Trade
                {
                    TradeId = Guid.NewGuid(),
                    TraderId = Guid.NewGuid(),
                    CommodityId = Guid.NewGuid(),
                    PricePerUnit = 200.00m,
                    Quantity = 25,
                    IsBuy = true,
                    Expiry = DateTime.Now.AddDays(45),
                    CreatedAt = DateTime.Now,
                    Bourse = "CME",
                    MitigationId = Guid.NewGuid(),
                    IsOpen = true,
                    Contract = "DEF789"
                }
            };
    }
}}
