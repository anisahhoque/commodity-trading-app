namespace CommodityTradingApp.Models
{
    public partial class TraderAccount
    {
        public Guid TraderId { get; set; }

        public Guid UserId { get; set; }

        public decimal Balance { get; set; }

        public string AccountName { get; set; } = null!;

        public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();

        public virtual User User { get; set; } = null!;
    }

}
