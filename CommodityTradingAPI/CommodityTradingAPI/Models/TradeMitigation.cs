using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class TradeMitigation
{
    public Guid MitigationId { get; set; }

    public decimal? SellPointProfit { get; set; }

    public decimal? SellPointLoss { get; set; }

    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}
