namespace CommodityTradingApp.Models
{
    public class TraderDetailsViewModel
    {
        public Trader Trader { get; set; }
        public List<Trade> ActiveTrades { get; set; }
        public List<Trade> HistoricalTrades { get; set; }

    }
}
