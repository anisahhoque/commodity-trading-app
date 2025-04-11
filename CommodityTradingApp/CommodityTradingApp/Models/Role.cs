namespace CommodityTradingApp.Models
{
    public class Role
    {
        public Guid RoleId { get; set; }

        public string RoleName { get; set; } = null!;

        public virtual ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
    }
}
