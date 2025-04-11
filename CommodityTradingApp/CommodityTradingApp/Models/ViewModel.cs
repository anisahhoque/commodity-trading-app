﻿using System;
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
}
