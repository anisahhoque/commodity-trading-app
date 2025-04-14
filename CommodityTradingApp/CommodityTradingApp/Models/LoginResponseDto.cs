namespace CommodityTradingApp.Models
{
    public class LoginResponseDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
    }
}
