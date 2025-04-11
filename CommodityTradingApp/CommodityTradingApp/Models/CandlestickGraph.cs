using System;
using ChartDirector;
using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace CSharpChartExplorer
{
    // "OUR API URL/api/commodity/{commodityName}/{timePeriod}/{startTime}/{endTime}"
    public class candlestick
    {
        static string CreateChartHtml(string commodityName, string timePeriod, string startTime, string endTime)
        {
            var data = GET [OUR API URL/ api / commodity /{ commodityName}/{ timePeriod}/{ startTime}/{ endTime} ]

            // Unpack data into high, low, open, close

            // Count length map to labels



            // Chart data
            double[] highData = 
                { 2043, 2039, 2076, 2064, 2048, 2058, 2070, 2033, 2027, 2029, 2071, 2085, 2034, 2031, 2056, 2128, 2180, 2183, 2192, 2213, 2230, 2281, 2272 };
            double[] lowData = { 1931, 1921, 1985, 2028, 1986, 1994, 1999, 1958, 1943, 1944, 1962, 2011, 1975, 1962, 1928, 2059, 2112, 2103, 2151, 2127, 2123, 2152, 2212 };
            double[] openData = { 2000, 1957, 1993, 2037, 2018, 2021, 2045, 2009, 1959, 1985, 2008, 2048, 2006, 2010, 1971, 2080, 2116, 2137, 2170, 2172, 2171, 2191, 2240 };
            double[] closeData = { 1950, 1991, 2026, 2029, 2004, 2053, 2011, 1962, 1987, 2019, 2040, 2016, 1996, 1985, 2006, 2113, 2142, 2167, 2158, 2201, 2188, 2231, 2242 };
            string[] labels = { "Mon 1", "Tue 2", "Wed 3", "Thu 4", "Fri 5", "Mon 8", "Tue 9", "Wed 10", "Thu 11", "Fri 12", "Mon 15", "Tue 16", "Wed 17", "Thu 18", "Fri 19", "Mon 22", "Tue 23", "Wed 24", "Thu 25", "Fri 26", "Mon 29", "Tue 30", "Wed 31" };

            // Create chart
            XYChart c = new XYChart(600, 350);
            c.setPlotArea(50, 25, 500, 250).setGridColor(0xc0c0c0, 0xc0c0c0);
            c.addTitle($"{commodityName} on Jan 2001"); // change to be relevant date
            c.addText(50, 25, $"{commodityName}", "Arial Bold", 12, 0x4040c0);
            c.xAxis().setTitle("Jan 2001"); // change this to be relevant
            c.xAxis().setLabels(labels).setFontAngle(45);
            c.yAxis().setTitle($"{commodityName} Price");
            c.setYAxisOnRight(true);
            CandleStickLayer layer = c.addCandleStickLayer(highData, lowData, openData, closeData, 0x00ff00, 0xff0000);
            layer.setLineWidth(2);

            // Create base64 image
            byte[] imageBytes = c.makeChart2(Chart.PNG);
            string base64Image = Convert.ToBase64String(imageBytes);// Change the below html to correspond to relevant code
            string imageMap = c.getHTMLImageMap("clickable", "",
                "title='{xLabel} Jan 2001\nHigh:{high}\nOpen:{open}\nClose:{close}\nLow:{low}'");

            // Assemble HTML
            string html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <title>Candlestick Chart</title>
                </head>
                <body>
                    <h2>Candlestick Chart</h2>
                    <img src='data:image/png;base64,{base64Image}' usemap='#clickable' border='0'>
                    {imageMap}
                </body>
                </html>";

            return html;
        }
    }
}
