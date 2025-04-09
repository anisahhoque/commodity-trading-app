using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class VwUserRole
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string RoleName { get; set; } = null!;
}
