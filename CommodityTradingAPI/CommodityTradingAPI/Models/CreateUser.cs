namespace CommodityTradingAPI.Models
{
    public class CreateUser
    {

        public string Username { get; set; } = null!;

        public string PasswordRaw { get; set; } = null!;

        public string Country { get; set; }

    }
}
