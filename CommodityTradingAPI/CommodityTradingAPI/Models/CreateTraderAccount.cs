namespace CommodityTradingAPI.Models
{
    public class CreateTraderAccount
    {
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public string AccountName { get; set; } = string.Empty;
    }
}
