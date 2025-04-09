using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class VwTraderTrade
{
    public Guid TradeId { get; set; }

    public Guid TraderId { get; set; }

    public string CommodityName { get; set; } = null!;

    public long PricePerUnit { get; set; }

    public byte Quantity { get; set; }

    public bool IsBuy { get; set; }

    public DateTime Expiry { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Bourse { get; set; } = null!;

    public long? SellPointProfit { get; set; }

    public long? SellPointLoss { get; set; }

    public bool IsOpen { get; set; }

    public string Contract { get; set; } = null!;
}
