namespace CommodityTradingApp.Models.DTOs
{
    public class CreateTradeDto
    {

        public Guid TraderId { get; set; }
        public Guid CommodityId { get; set; }
        public int Quantity { get; set; }
        //public List<string>? Mitigations { get; set; }
        public string IsBuy { get; set; }

        public string IsOpen { get; set; }

        public string contract { get; set; }
        public string Bourse { get; set; }

        //Not sure how this one should be done, WIP atm
        //public virtual TradeMitigation Mitigations { get; set; }

    }
}
