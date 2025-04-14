namespace CommodityTradingAPI.Models.DTOs
{
    public class TraderAccountPortfolioDto
    {

        public string AccountName   { get; set; }
        public string UserName { get; set; }

        public decimal Balance { get; set; }

        public ICollection<Trade> OpenAccountTrades { get; set; }



    }
}
