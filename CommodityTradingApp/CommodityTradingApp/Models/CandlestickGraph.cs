using System;
using System.Net.Http;
using System.Collections.Generic;
using ChartDirector;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CommodityTradingApp
{
    public class Candlestick
    {
        private readonly IConfiguration _configuration;

        // Constructor that takes IConfiguration to be injected
        public Candlestick(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateChartHtmlAsync(string commodityName, string timePeriod, string startTime, string endTime)
        {
            string apiUrl = $"{_configuration["api"]}commodity/{commodityName}/{timePeriod}/{startTime}/{endTime}";

            List<CandleData> candles;

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch data from API: {response.StatusCode}, {apiUrl}");
                }

                string jsonString = await response.Content.ReadAsStringAsync();
                candles = JsonConvert.DeserializeObject<List<CandleData>>(jsonString);
                candles.Reverse();
            }

            if (candles == null || candles.Count == 0)
            {
                throw new Exception("No candle data returned from API.");
            }

            // Unpack data
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            int count = candles.Count;
            double[] highData = new double[count];
            double[] lowData = new double[count];
            double[] openData = new double[count];
            double[] closeData = new double[count];
            string[] labels = new string[count];

            for (int i = 0; i < count; i++)
            {
                var candle = candles[i];

                highData[i] = candle.high;
                lowData[i] = candle.low;
                openData[i] = candle.open;
                closeData[i] = candle.close;

                // Convert Unix timestamp to DateTime and format
                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(candle.time);
                labels[i] = dto.ToLocalTime().ToString("ddd d MMM HH:mm");
            }

            commodityName = textInfo.ToTitleCase(commodityName);

            // Create chart
            XYChart c = new XYChart(1200, 800); // Size of chart
            c.setPlotArea(50, 25, 900, 600).setGridColor(0xc0c0c0, 0xc0c0c0); // Size of the plot itself (the actual graph), last two numbers are size
            c.addTitle($"{commodityName} Chart");
            c.addText(50, 25, $"{commodityName}", "Arial Bold", 12, 0x4040c0);
            c.xAxis().setTitle($"Data in steps of {timePeriod}");
            c.xAxis().setLabels(labels).setFontAngle(45);
            c.yAxis().setTitle($"{commodityName} Price");
            c.setYAxisOnRight(true);
            CandleStickLayer layer = c.addCandleStickLayer(highData, lowData, openData, closeData, 0x00ff00, 0xff0000);
            layer.setLineWidth(2);

            // Generate chart image
            byte[] imageBytes = c.makeChart2(Chart.PNG);
            string base64Image = Convert.ToBase64String(imageBytes);
            string imageMap = c.getHTMLImageMap("clickable", "",
                "title='{xLabel}\nHigh:{high}\nOpen:{open}\nClose:{close}\nLow:{low}'");

            // Make HTML
            string html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>{commodityName} Candlestick Chart</title>
                </head>
                <body>
                    <h2>{commodityName} Candlestick Chart</h2>
                    <img src='data:image/png;base64,{base64Image}' usemap='#clickable' border='0'>
                    {imageMap}
                </body>
                </html>";

            return html;
        }
    }

    public class CandleData
    {
        public double open { get; set; }
        public double low { get; set; }
        public double high { get; set; }
        public double close { get; set; }
        public long time { get; set; } // Unix time in seconds
    }
}
