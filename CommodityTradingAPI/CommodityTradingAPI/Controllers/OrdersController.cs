using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CommodityTradingAPI.Models.DTOs;
using CommodityTradingAPI.Services;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly CommoditiesDbContext _context;
        private readonly ExternalApiService _externalApiService;

        public OrdersController(CommoditiesDbContext context, ExternalApiService externalApiService)
        {
            _context = context;
            _externalApiService = externalApiService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Trade>>> Index([FromQuery] int page_num = 1, int page_size = 10)
        {
            if (page_num < 1)
            {
                return BadRequest("Invalid page number");
            }
            if (page_size < 1)
            {
                return BadRequest("Invalid page size");
            }
            if (page_num >= 1)
            {
                var trades = _context.Trades.ToListAsync();
                var result = trades.Result.Skip(page_size * (page_num - 1)).Take(page_size);
                if (result == null)
                {
                    return NotFound("No trades found");
                }
                return Ok(result);
            }
            return BadRequest("No trades found");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Trade>> Details(Guid id)
        {
            var trade = await _context.Trades.FindAsync(id);
            if (trade == null)
            {
                return NotFound("Trade not found");
            }
            return Ok(trade);
        }

        [HttpPost]
        public async Task<ActionResult<Trade>> Create([Bind("TraderId,CommoditiyId,Quantity,Mitigations,Bourse")] CreateTradeDto trade)
        {
            // Pass user-supplied trade details into new Trade object
            Trade tempTrade = new();

            var commodityName = tempTrade.Commodity.CommodityName;

            // Make request to external API to get the latest price of the commodity
            var price = await _externalApiService.GetCommodityPrice(commodityName);

            tempTrade.PricePerUnit = price;

            return Ok(tempTrade); 
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, bool IsOpen)
        {
            var trade = await _context.Trades.FindAsync(id);
            if (trade == null)
            {
                return NotFound("Trade not found");
            }
            if (!trade.IsOpen) 
            {
                return BadRequest("Trade is already closed.");
            } else
            {
                trade.IsOpen = false;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Trade closed successfully." });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var trade = await _context.Trades.FindAsync(id);
            if (trade == null)
            {
                return NotFound("Trade not found");
            }
            _context.Trades.Remove(trade);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Trade deleted successfully" });
        }
    }
}
