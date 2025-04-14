namespace CommodityTradingApp.Models
{
    public class CommodityPrice
    {

            public decimal Open { get; set; }
            public decimal Low { get; set; }
            public decimal High { get; set; }
            public decimal Close { get; set; }
            public long Volume { get; set; }
            public long Time { get; set; }  // Unix timestamp
        

    }
}
