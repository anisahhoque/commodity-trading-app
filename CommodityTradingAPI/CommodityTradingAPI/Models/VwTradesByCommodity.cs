using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class VwTradesByCommodity
{
    public string CommodityName { get; set; } = null!;

    public int? TotalBuyQuantity { get; set; }

    public long? TotalBuyValue { get; set; }

    public int? TotalSellQuantity { get; set; }

    public long? TotalSellValue { get; set; }
}
