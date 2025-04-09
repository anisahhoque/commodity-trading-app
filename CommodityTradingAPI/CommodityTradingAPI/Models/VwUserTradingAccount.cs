using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class VwUserTradingAccount
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string? Country { get; set; }

    public Guid TraderId { get; set; }

    public string AccountName { get; set; } = null!;

    public long Balance { get; set; }

    public string? CommodityName { get; set; }

    public Guid? TradeId { get; set; }

    public long? PricePerUnit { get; set; }

    public byte? Quantity { get; set; }

    public bool? IsBuy { get; set; }

    public DateTime? Expiry { get; set; }

    public DateTime? TradeCreatedAt { get; set; }

    public bool? IsOpen { get; set; }

    public string? Bourse { get; set; }

    public string? Contract { get; set; }
}
