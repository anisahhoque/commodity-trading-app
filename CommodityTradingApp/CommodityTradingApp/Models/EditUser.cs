using Microsoft.AspNetCore.Mvc.Rendering;

namespace CommodityTradingApp.Models
{
    public class EditUser
    {
        public Guid UserId { get; set; }

        public string Username { get; set; } = null!;

        public string? Password { get; set; }


        public byte CountryId { get; set; }


        public IEnumerable<SelectListItem> AllCountries { get; set; } = new List<SelectListItem>();

        public IEnumerable<SelectListItem> AllRoles { get; set; } = new List<SelectListItem>();


        public List<Guid> SelectedRoleIds { get; set; } = new List<Guid>();


        public string? CurrentCountryName { get; set; }
    }

    
}
