using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CommodityTradingAPI.Data;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class CountryController : ControllerBase
    {
        private readonly CommoditiesDbContext _context;

        public CountryController(CommoditiesDbContext context)
        {
            _context = context;
        }
        //GET: api/country
        [HttpGet]
        public IActionResult Index()
        {
            var countries = _context.Countries.ToList();

            return Ok(countries);
        }
    }
}
