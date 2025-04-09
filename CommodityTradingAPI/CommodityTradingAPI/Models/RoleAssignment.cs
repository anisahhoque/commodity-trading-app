using System;
using System.Collections.Generic;

namespace CommodityTradingAPI.Models;

public partial class RoleAssignment
{
    public Guid AssignmentId { get; set; }

    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public virtual Role Role { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
