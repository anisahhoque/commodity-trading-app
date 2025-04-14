using CommodityTradingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CommodityTradingApp.Controllers
{
    [Authorize]
    public class TradeController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private static List<Trade> trades = GetMockTrades();
        private static List<Commodity> commodities = GetMockCommodities();
        private readonly string _apiUrl;
        private readonly string _apiUrlCommodity;
        public TradeController(HttpClient httpClient, IConfiguration config )
        {
            _httpClient = httpClient;
            _config = config;
            _apiUrl = _config["api"] + "Trade/";
            _apiUrlCommodity = _config["api"] + "Commodity/";
        }
        public async Task<IActionResult> Index()
        {
            return View(trades);
        }
        [Authorize(Roles = "Manager")]
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
            var commodityPrices = new List<CommodityWithPriceViewModel>();
            var response = await _httpClient.GetAsync(_apiUrlCommodity);
            long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long unixMidnight = new DateTimeOffset(
                                    DateTime.UtcNow.Date, 
                                    TimeSpan.Zero         
                                ).ToUnixTimeSeconds();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var commodities = JsonConvert.DeserializeObject<List<Commodity>>(json);
                foreach (var commodity in commodities)
                {
                    var currentPrice = await _httpClient.GetAsync(_apiUrlCommodity + commodity.CommodityName);
                    var priceResponse = await _httpClient.GetAsync(_apiUrlCommodity + $"{commodity.CommodityName}/1m/{unixMidnight}/{unixTime}");
                    if (priceResponse.IsSuccessStatusCode)
                    {
                        var currentPriceJson = await currentPrice.Content.ReadAsStringAsync();
                        var currentPriceObj = JsonConvert.DeserializeObject<CurrentCommodityPrice>(currentPriceJson);

                        var priceJson = await priceResponse.Content.ReadAsStringAsync();
                        var priceList = JsonConvert.DeserializeObject<List<CommodityPrice>>(priceJson);

                        var currPrice = currentPriceObj.Price;
                        var latest = priceList?.FirstOrDefault();
                        var midnight = priceList?.Last();
                        commodityPrices.Add(new CommodityWithPriceViewModel
                        {
                            Commodity = commodity,
                            Price = currentPriceObj.Price,
                            High = latest?.High ?? 0,
                            Low = latest?.Low ?? 0,
                            AbsoluteChange = (midnight != null) ? latest.Close - midnight.Open : 0,
                            RelativeChange = (midnight != null && midnight.Open != 0)
                        ? ((latest.Close - midnight.Open) / midnight.Open) * 100
                        : 0
                        });
                    }
                }
                return View(commodityPrices);
                
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Unable to retrieve commodities from the API");
                return View(new List<CommodityWithPriceViewModel>());
            }
          
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

        [Authorize(Roles = "Manager")]
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
