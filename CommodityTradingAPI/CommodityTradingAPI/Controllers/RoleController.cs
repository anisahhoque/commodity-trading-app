using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommodityTradingAPI.Data;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RoleController : ControllerBase
    {
        private readonly CommoditiesDbContext _context;

        public RoleController(CommoditiesDbContext context)
        {
            _context = context;
        }
        //GET: api/role
        [HttpGet]
        public IActionResult Index()
        {
            var roles= _context.Roles.ToList();

            //just added comment
            return Ok(roles);
        }


    }
}
