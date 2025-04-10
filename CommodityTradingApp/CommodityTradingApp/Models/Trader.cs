﻿namespace CommodityTradingApp.Models
{
    public class Trader
    {
        public Guid Id { get; set; }
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public Guid UserId { get; set; }

    }
}
