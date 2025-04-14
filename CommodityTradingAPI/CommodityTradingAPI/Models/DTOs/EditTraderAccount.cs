namespace CommodityTradingAPI.Models.DTOs
{
    public class EditTraderAccount
    {
        public Guid TraderId { get; set; }
        public string AccountName { get; set; } = null!;
    }
}
