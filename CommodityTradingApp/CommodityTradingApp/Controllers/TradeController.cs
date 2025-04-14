using CommodityTradingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Controllers
{
    [Authorize]
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
        public async Task<IActionResult> DisplayCommodities()
        {
            //call get details for each commodity in commodities
            return View(commodities);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTrade(Guid CommodityId, int Quantity, bool IsBuy, decimal Price)
        {
            var newTrade = new Trade
            {
                TradeId = Guid.NewGuid(),
                TraderId = Guid.NewGuid(), 
                CommodityId = CommodityId,
                PricePerUnit = 100.0m, // will obtain price using api
                Quantity = Quantity,
                IsBuy = IsBuy,
                Expiry = DateTime.Now.AddDays(30),
                CreatedAt = DateTime.Now,
                Bourse = "NYSE", //grab from api
                MitigationId = Guid.NewGuid(),
                IsOpen = true,
                Contract = "ABC01"
            };
            trades.Add(newTrade);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> DeleteTrade(Guid id)
        {
            //make a call to delete trade api
            var tradeToDelete = trades.FirstOrDefault(t => t.TradeId == id);
            if (tradeToDelete != null)
            {
                trades.Remove(tradeToDelete);
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> UpdateTrade(Guid id)
        {
            var tradeToUpdate = trades.FirstOrDefault(t => t.TradeId == id);
            if (tradeToUpdate != null)
            {
                tradeToUpdate.IsOpen = false; 
            }
            //make a call to update trade
            return RedirectToAction("Index");
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
        private static List<Trade> GetMockTrades()
        {
            return new List<Trade>
            {
                new Trade
                {
                    TradeId = Guid.NewGuid(),
                    TraderId = Guid.NewGuid(),
                    CommodityId = Guid.Parse("7c472314-bb42-4e02-86c1-7fc28e6de3b6"),
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
                    CommodityId = Guid.Parse("92d2573e-3ad1-4f38-8859-9d4d9e9e3a0e"),
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
                    CommodityId = Guid.Parse("f8c3618a-53a8-4097-871f-50a924bdcf1c"),
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
