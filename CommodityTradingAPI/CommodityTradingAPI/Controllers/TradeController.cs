using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using CommodityTradingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using ILogger = CommodityTradingAPI.Services.ILogger;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeController : ControllerBase
    {
        
        private readonly CommoditiesDbContext _context;
        private readonly ExternalApiService _api;
        private ILogger _auditLogService;

        public TradeController(CommoditiesDbContext context, ExternalApiService api, ILogger logger)
        {
            _context = context;
            _api = api;
            _auditLogService = logger;
        }

        
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Index()
        {
            return Ok();
            //all trades?? or maybe just not needed
            //shows all trades (if you are the manager???)

        }


        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> MakeTrade([Bind("TraderId, CommodityId, Quantity, IsBuy, Expiry, Bourse, MitigationId, IsOpen, Contract")] Trade trade)
        {
            
            //bind request body to A Trade object

            var account = await _context.TraderAccounts.FirstOrDefaultAsync(x => x.TraderId == trade.TraderId);
            var commodity = await _context.Commodities.FirstOrDefaultAsync(x => x.CommodityId == trade.CommodityId);

            if (account is null)
            {
                var auditLog = new AuditLog
                {
                    EntityName = "Trade",
                    Action = "Failed Create",
                    Timestamp = DateTime.UtcNow,
                    ChangedBy = "CHANGE ME",
                    Details = $"User {Request.Headers} attempted to create a trade but failed due to a null account."
                };

                await _auditLogService.LogChangeAsync(auditLog);
                return BadRequest("Failed to create trade due to a null account.");
            }

            if (commodity is null)
            {
                var auditLog = new AuditLog
                {
                    EntityName = "Trade",
                    Action = "Failed Create",
                    Timestamp = DateTime.UtcNow,
                    ChangedBy = "CHANGE ME",
                    Details = $"User {"CHANGEME"} attempted to create a trade but failed due to a null commodity."
                };

                await _auditLogService.LogChangeAsync(auditLog);
                return BadRequest("Failed to create trade due to a null commodity.");
            }

            if (trade.Quantity <= 0)
            {
                var auditLog = new AuditLog
                {
                    EntityName = "Trade",
                    Action = "Failed Create",
                    Timestamp = DateTime.UtcNow,
                    ChangedBy = "CHANGE ME",
                    Details = $"User {"CHANGEME"} attempted to create a trade but failed due to a quantity of 0 or less."
                };

                await _auditLogService.LogChangeAsync(auditLog);
                return BadRequest("Failed to create trade due to a 0 quantity purchased.");
            }

            var priceOfCommodity = await _api.GetCommodityPrice(commodity.CommodityName);

            if ((account.Balance - priceOfCommodity * trade.Quantity) < 0)
            {
                var auditLog = new AuditLog
                {
                    EntityName = "Trade",
                    Action = "Failed Create",
                    Timestamp = DateTime.UtcNow,
                    ChangedBy = "CHANGE ME",
                    Details = $"User {"CHANGEME"} attempted to create a trade but failed due putting balance in negative."
                };

                await _auditLogService.LogChangeAsync(auditLog);
                return BadRequest("Failed to create trade due to account not having enough money for trade.");
            }

            //Add any other fields that need to be added to trade object

            var createdAt = DateTime.UtcNow;

            trade.CreatedAt = createdAt;
            trade.PricePerUnit = priceOfCommodity;
            
            //Store trade in database

            await _context.Trades.AddAsync(trade);
            await _context.SaveChangesAsync();


            return CreatedAtAction(nameof(GetTradeById), new { id = trade.TradeId }, trade);

            //Smiel and bee happy :P

        }


        [HttpPatch]//may have to change to post am not sure yet
        public async Task<IActionResult> UpdateTrade()
        {
            return Ok();
            //really only thing that changes about a trade is whether the user has opened or closed it
        }

        public async Task<IActionResult> DeleteTrade()
        {
            //Good to have the option but do we ever delete a trade???? <- no
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTradeById(Guid id)
        {
            var trade = await _context.Trades.FindAsync(id);
            if (trade == null)
            {
                return NotFound();
            }
            return Ok(trade);
        }


    }
}
