using System;
using CommodityTradingApp.Models;

namespace CommodityTradingApp.ViewModels

//ViewModel to hold trader details and trades to pass into views
{
    public class TraderDetailsViewModel
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

    public class EditTraderViewModel
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
    }

    public class UserDetailsViewModel
    {
        public User User { get; set; }
        public List<Trader> Traders { get; set; }
    }

    public class UserCreateViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CountryName { get; set; }
    }

    public class UserEditViewModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public byte CountryId { get; set; }
    }
}
