using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CommodityTradingAPI.Models;

public partial class User
{
    
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public byte CountryId { get; set; }

    
    public Country Country { get; set; } = null!;
    
    public virtual ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
    
    public virtual ICollection<TraderAccount> TraderAccounts { get; set; } = new List<TraderAccount>();
}
