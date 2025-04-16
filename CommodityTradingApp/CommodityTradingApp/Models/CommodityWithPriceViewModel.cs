//using CommodityTradingApp.Models;


namespace CommodityTradingApp.Models
{
    public class CommodityWithPriceViewModel
    {
        public Commodity Commodity { get; set; }
        public decimal Price { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal AbsoluteChange { get; set; }
        public decimal RelativeChange { get; set; }
    }

}
