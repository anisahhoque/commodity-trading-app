using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class Country
{
    public byte CountryId { get; set; }

    public string? CountryName { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
