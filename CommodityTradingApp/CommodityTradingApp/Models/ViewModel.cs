namespace CommodityTradingApp.Models

//ViewModel to hold trader details and trades to pass into views
{
    public class ViewModel
    {
        public Trader Trader { get; set; }
        public List<Trade> ActiveTrades { get; set; }
        public List<Trade> HistoricalTrades { get; set; }

    }

    public class CreateTraderViewModel
    {
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public Guid UserId { get; set; }
    }
}
