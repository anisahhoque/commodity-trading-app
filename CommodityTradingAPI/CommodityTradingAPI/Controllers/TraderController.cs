using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using CommodityTradingAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TraderController : ControllerBase
    {
        private readonly CommoditiesDbContext _context;
        private readonly ExternalApiService _externalApiService;

        public TraderController(CommoditiesDbContext context, ExternalApiService externalApiService)
        {
            _context = context;
            _externalApiService = externalApiService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var traders = await _context.TraderAccounts.ToListAsync();
            if (traders == null)
            {
                return NotFound("No traders found");
            }
            return Ok(traders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var trader = await _context.TraderAccounts.FindAsync(id);
            if (trader == null)
            {
                return NotFound("Trader not found");
            }
            return Ok(trader);
        }

        [HttpGet("{id}/portfolio")]
        public async Task<IActionResult> TradeHistory(Guid id)

            //returns the trading accounts balance, plus all the traders' currently open trades
        {
            var trader = await _context.TraderAccounts.FindAsync(id);
            var user = trader.User;


            //var trades = trader.Trades.ToList();
            if (trader == null)
            {
                return NotFound("Trader not found");
            }
            var trades = await _context.Trades.Where(t => t.TraderId == id && t.IsOpen).ToListAsync();
            if (trades == null)
            {
                return NotFound("No open trades found for this trader");
            }
            return Ok(trades);
        }

        //should we also have an endpoint just to get a trading accounts balance??? maybe not needed


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TraderAccount traderAccount)
        {
            if (traderAccount == null)
            {
                return BadRequest("Invalid trader account");
            }
            _context.TraderAccounts.Add(traderAccount);
            await _context.SaveChangesAsync();
            return Ok(traderAccount);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] TraderAccount traderAccount)
        {
            if (id != traderAccount.TraderId)
            {
                return BadRequest("Trader ID invalid.");
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var trader = await _context.TraderAccounts.FindAsync(id);
            if (trader == null)
            {
                return NotFound("Trader not found");
            }
            _context.TraderAccounts.Remove(trader);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
