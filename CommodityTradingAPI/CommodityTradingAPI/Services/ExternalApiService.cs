using Newtonsoft.Json;
using CommodityTradingAPI.Models;
using System.Net.Http.Headers;

namespace CommodityTradingAPI.Services
{
    public class ExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        public ExternalApiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri("https://api.api-ninjas.com/v1");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration["X-Api-Key"]);
        }

        public async Task<decimal> GetCommodityPrice (string commodityName)
        {
            var response = await _httpClient.GetAsync($"/commodity?name={commodityName}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ExternalCommodity>(json);
                return data.price;  
                
            } else
            {
                throw new Exception("Price not found.");
            }
        }
    }
}
