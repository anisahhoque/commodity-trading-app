namespace CommodityTradingAPI.Models.DTOs
{
    public class CreateTradeDto
    {
        public Guid TraderId { get; set; }
        public Guid CommodityId { get; set; }
        public byte Quantity { get; set; }
        public List<string>? Mitigations { get; set; }
        public string Bourse { get; set; }
    }
}
