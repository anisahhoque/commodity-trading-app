using Microsoft.AspNetCore.Mvc;

namespace CommodityTradingApp.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int CountryID { get; set; }
    }
}

