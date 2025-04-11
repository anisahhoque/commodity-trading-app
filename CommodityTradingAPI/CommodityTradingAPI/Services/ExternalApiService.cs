using Newtonsoft.Json;
using CommodityTradingAPI.Models;
using System.Net.Http.Headers;
using System.Net.Http;

namespace CommodityTradingAPI.Services
{
    public class ExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        public ExternalApiService(HttpClient httpClient, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri("https://api.api-ninjas.com/v1");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _configuration["X-Api-Key"]);

            _httpClientFactory = httpClientFactory;
        }

        public async Task<decimal> GetCommodityPrice(string commodityName)
        {
            var apiKey = _configuration["X-Api-Key"];
            //if (string.IsNullOrEmpty(apiKey))




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
                    var data = JsonConvert.DeserializeObject<ExternalCommodity>(content);

                    return data.price;
                }



                //var response = await _httpClient.GetAsync($"/commodityprice?name={commodityName}");
                //if (response.IsSuccessStatusCode)
                //{
                //    var json = await response.Content.ReadAsStringAsync();
                //    var data = JsonConvert.DeserializeObject<ExternalCommodity>(json);
                //    return data.price;  

                //} else
                //{
                //    throw new Exception("Price not found.");
                //}
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
            return 1;
        }
    }

}
