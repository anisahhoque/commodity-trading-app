using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class Trade
{
    public Guid TradeId { get; set; }

    public Guid TraderId { get; set; }

    public Guid CommodityId { get; set; }

    public decimal PricePerUnit { get; set; }

    public byte Quantity { get; set; }

    public bool IsBuy { get; set; }

    public DateTime Expiry { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Bourse { get; set; } = null!;

    public Guid MitigationId { get; set; }

    public bool IsOpen { get; set; }

    public string Contract { get; set; } = null!;

    public virtual Commodity Commodity { get; set; } = null!;

    public virtual TradeMitigation Mitigation { get; set; } = null!;

    public virtual TraderAccount Trader { get; set; } = null!;
}
