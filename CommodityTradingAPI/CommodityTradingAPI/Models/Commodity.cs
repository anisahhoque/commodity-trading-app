using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class Commodity
{
    public Guid CommodityId { get; set; }

    public string CommodityName { get; set; } = null!;

    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}
