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
            double[] volumeData = new double[count];
            string[] labels = new string[count];

            for (int i = 0; i < count; i++)
            {
                var candle = candles[i];

                highData[i] = candle.high;
                lowData[i] = candle.low;
                openData[i] = candle.open;
                closeData[i] = candle.close;
                volumeData[i] = candle.volume;

                // Convert Unix timestamp to DateTime and format
                DateTimeOffset dto = DateTimeOffset.FromUnixTimeSeconds(candle.time);
                labels[i] = dto.ToLocalTime().ToString("d MMM HH:mm");
            }

            commodityName = textInfo.ToTitleCase(commodityName);

            // Create the chart
            XYChart c = new XYChart(1200, 800);
            c.addTitle($"{commodityName} Candlestick Chart");

            // Set plot area
            PlotArea plotArea = c.setPlotArea(70, 50, 1000, 600);
            plotArea.setGridColor(0xc0c0c0, 0xc0c0c0);

            // X-Axis setup
            c.xAxis().setLabels(labels).setFontAngle(45);
            c.xAxis().setLabelStep(10); // Increase this to have more blank tics
            c.xAxis().setTitle($"Data in steps of {timePeriod}");

            // Primary Y-Axis for price
            c.yAxis2().setTitle($"{commodityName} Price");
            c.yAxis2().setAutoScale(0.05, 0.3); // Visual separation

            // Add candlestick layer
            CandleStickLayer candleLayer = c.addCandleStickLayer(highData, lowData, openData, closeData, 0x00ff00, 0xff0000);
            candleLayer.setUseYAxis2();
            candleLayer.setLineWidth(2);

            // Secondary Y-Axis for volume
            BarLayer volumeLayer = c.addBarLayer(volumeData, 0x9999ff);
            volumeLayer.setBarWidth(8);

            c.yAxis().setTitle($"{commodityName} Volume");
            c.yAxis().setAutoScale(0.75, 0.05);

            // Generate chart image
            byte[] imageBytes = c.makeChart2(Chart.PNG);
            string base64Image = Convert.ToBase64String(imageBytes);
            string imageMap = c.getHTMLImageMap("clickable", "",
                "title='{xLabel}\nHigh:{high}\nOpen:{open}\nClose:{close}\nLow:{low}\nVolume:{value}'");

            // Build HTML
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
        public double volume { get; set; } // Volume for the histogram
    }
}
