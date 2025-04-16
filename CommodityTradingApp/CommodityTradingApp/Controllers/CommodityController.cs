using System.Runtime.InteropServices;
using CommodityTradingApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Controllers
{
    [Authorize]
    public class CommodityController : Controller

    {
        private readonly HttpClient _httpClient;
        private static List<Commodity> commodities;
        private readonly IConfiguration _configuration;
        private readonly Candlestick _candlestick;

        public CommodityController(HttpClient httpClient, IConfiguration configuration, Candlestick candlestick)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _candlestick = candlestick;
            string comodEndpoint = "commodity";
            _httpClient.BaseAddress = new Uri(_configuration["api"] + comodEndpoint);

            //commodities = GetMockCommodities();


        }

        public async Task<IActionResult> Index()
        {
            var result = await _httpClient.GetAsync(_httpClient.BaseAddress);
            if (result!= null)
            {
                var allCommods = await result.Content.ReadAsAsync<List<Commodity>>();

                // Call CreateChartHtmlAsync for the candlestick chart
                var chartHtml = await _candlestick.CreateChartHtmlAsync("gold", "5m", "1744600000", "1744622096");

                // Pass chartHtml to the view
                ViewBag.ChartHtml = chartHtml;

                return View(allCommods);
            }

            //call get list of commodities
            return View(result);
        }
        //oigrf
        //private static List<Commodity> GetMockCommodities()
        //{
        //    return new List<Commodity>
        //    {
        //        new Commodity
        //        {
        //            CommodityId = Guid.Parse("7c472314-bb42-4e02-86c1-7fc28e6de3b6"),
        //            CommodityName = "Gold"
                    
        //        },
        //        new Commodity
        //        {
        //            CommodityId = Guid.Parse("92d2573e-3ad1-4f38-8859-9d4d9e9e3a0e"),
        //            CommodityName = "Oil"

        //        },
        //        new Commodity
        //        {
        //            CommodityId = Guid.Parse("f8c3618a-53a8-4097-871f-50a924bdcf1c") ,
        //            CommodityName = "Silver"

        //        }
        //    };
        //}
    }
}
