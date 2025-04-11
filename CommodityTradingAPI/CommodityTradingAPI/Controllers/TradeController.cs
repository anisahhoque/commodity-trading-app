using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using CommodityTradingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeController : ControllerBase
    {
        
        private readonly CommoditiesDbContext _context;
        private readonly ExternalApiService _api;

        public TradeController(CommoditiesDbContext context, ExternalApiService api)
        {
            _context = context;
            _api = api; 
        }

        
        [HttpGet]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Index()
        {
            
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
                    Action = "Create",
                    Timestamp = DateTime.UtcNow,
                    User = _context.User,
                    Details = $"User {newUser.Username} attempted to create a trade but failed due to a null account."
                };

                await _auditLogService.LogChangeAsync(auditLog);
                return response.FailedToCreate;
            }

            if (commodity is null)
            {
                var auditLog = new AuditLog
                {
                    EntityName = "User",
                    Action = "Create",
                    Timestamp = DateTime.UtcNow,
                    Details = $"User {newUser.Username} was created."
                };

                await _auditLogService.LogChangeAsync(auditLog);
                return response.FailedToCreate;
            }

            //if ((account.balance - priceOfCommod (from API) * quant) < 0) { }


            //Call external api to get most recent price of commdoity

            //Add any other fields that need to be added to trade object

            var createdAt = DateTime.UtcNow;

            trade.CreatedAt = createdAt;

            //Store trade in database

            await _context.Trades.AddAsync(trade);

            return Created();

            // Check is valid commodity

            // Check is valid trader account

            // Check does not put user in negative balance

            //adds a trade to a user's profile

            //Smiel and bee happy :P

        }


        [HttpPatch]//may have to change to post am not sure yet
        public async Task<IActionResult> UpdateTrade()
        {
            //really only thing that changes about a trade is whether the user has opened or closed it
        }

        public async Task<IActionResult> DeleteTrade()
        {
            //Good to have the option but do we ever delete a trade???? <- no
            return Ok();
        }

        


    }
}
