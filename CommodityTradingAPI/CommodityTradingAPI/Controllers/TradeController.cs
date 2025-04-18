﻿using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using CommodityTradingAPI.Models.DTOs;
using CommodityTradingAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Newtonsoft.Json.Bson;
//using ILogger = CommodityTradingAPI.Services.ILogger;

namespace CommodityTradingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradeController : ControllerBase
    {
        
        private readonly CommoditiesDbContext _context;
        private readonly ExternalApiService _api;
        private CommodityTradingAPI.Services.ILogger _auditLogService;

        public TradeController(CommoditiesDbContext context, ExternalApiService api, CommodityTradingAPI.Services.ILogger logger)
        {
            _context = context;
            _api = api;
            _auditLogService = logger;
        }

        
        //Add route to get all trades for a specific trader (by Id)


        [HttpGet]
        //[Authorize(Roles = "Manager")]
        public async Task<IActionResult> Index()
        {
            var allTrades = await _context.Trades.ToListAsync();

            return Ok(allTrades);
            
            //all trades?? or maybe just not needed
            //shows all trades (if you are the manager???)

        }


        [HttpPost]
        //[Authorize(Roles = "Manager")]
        //Trade trade
        public async Task<IActionResult> MakeTrade(CreateTradeDto tempTrade)
        {

            Trade trade = new Trade
            {
                TraderId = tempTrade.TraderId,
                CommodityId = tempTrade.CommodityId,
                Quantity = (byte)tempTrade.Quantity,
                Bourse = tempTrade.Bourse,
                CreatedAt = DateTime.UtcNow,
                Contract = tempTrade.contract
            };

            //TODO: may need to add separate logic if trade is a sell???

            var account = await _context.TraderAccounts.FirstOrDefaultAsync(x => x.TraderId == trade.TraderId);
            var commodity = await _context.Commodities.FirstOrDefaultAsync(x => x.CommodityId == trade.CommodityId);


            //TODO: separate out validation logic
            await ValidateCreatedTrade(account, commodity, trade);

            var priceOfCommodity = await _api.GetCommodityPrice(commodity.CommodityName);


            //return BadRequest("Failed to create trade due to a null commodity.");

            //Add any other fields that need to be added to trade object

            var createdAt = DateTime.UtcNow;
            var expiry = createdAt.AddDays(10); 

            trade.CreatedAt = createdAt;
            trade.PricePerUnit = priceOfCommodity;
            trade.Expiry = expiry;
            trade.IsOpen = tempTrade.IsOpen == "True";
            trade.IsBuy = tempTrade.IsBuy == "True";

            //TODO: allow user to create their own trade mitigation for the position




            //Store trade in database
            TradeMitigation tm = new()
            {
                SellPointLoss = tempTrade.Mitigations.SellPointLoss,
                SellPointProfit = tempTrade.Mitigations.SellPointProfit
            };

            await _context.TradeMitigations.AddAsync(tm);

            await _context.SaveChangesAsync();

            var x = await _context.TradeMitigations.ToListAsync();
            var tmId = x.LastOrDefault().MitigationId;

            trade.MitigationId = tmId;

            await _context.Trades.AddAsync(trade);
            await _context.SaveChangesAsync();

            //await _auditLogService.CreateNewLogAsync(
            //    "Trade", // Entity
            //    "Create (Open)", // Change
            //    trade.TraderId.ToString() + " which is a trade account of " +
            //    await _context.TraderAccounts
            //    .Where(t => t.TraderId == trade.TraderId)
            //    .Select(t => new
            //    { t.User.UserId, t.User.Username })
            //    .FirstOrDefaultAsync(), // Who changed
            //    $"User {await _context.TraderAccounts // Full detail
            //    .Where(t => t.TraderId == trade.TraderId)
            //    .Select(t => new
            //    { t.User.UserId, t.User.Username })
            //    .FirstOrDefaultAsync()} created a {(trade.IsBuy ? "buy" : "sell")} of {trade.Quantity} of {commodity} on their account" +
            //    $"{await _context.TraderAccounts
            //    .Where(t => t.TraderId == trade.TraderId)
            //    .Select(t => t.AccountName)
            //    .FirstOrDefaultAsync()}",
            //    trade.Trader.User.Country.CountryName == "Russia"
            //    );

            //return CreatedAtAction(nameof(GetTradeById), new { id = trade.TradeId }, trade);
            return Created();


        }


        [HttpPatch]//may have to change to post am not sure yet
        public async Task<IActionResult> CloseTrade(Guid id)
        {
            //Find trade

            var trade = await _context.Trades.FirstOrDefaultAsync(x => x.TradeId == id);

            if (trade == null)
                return BadRequest("Trade could not be found");

            trade.IsOpen = false;

            //get commodoties price, and update user's balance accordingly

            var startPrice = trade.PricePerUnit;
            var currentPrice = await _api.GetCommodityPrice(trade.Commodity.CommodityName);

            var total = (currentPrice - startPrice) * trade.Quantity;


            trade.Trader.Balance += total;

            await _context.SaveChangesAsync();

            await _auditLogService.CreateNewLogAsync(
                "Trade", // Entity
                "Create (Close)", // Change
                trade.TraderId.ToString() + " which is a trade account of " + trade.Trader.User.Username, // Who changed
                $"User {trade.Trader.User.Username} created a sell of {trade.Quantity} of {trade.Commodity.CommodityName} on their account" +
                $"{trade.Trader.AccountName}",
                trade.Trader.User.Country.CountryName == "Russia"
                );

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

        [HttpGet("trader/{traderId}")]
        public async Task<IActionResult> GetTradesByTraderId(Guid traderId)
        {
            var trades = await _context.Trades
                .Where(t => t.TraderId == traderId)
                .ToListAsync();

            if (trades == null)
            {
                return NotFound("No trades found for the given trader.");
            }

            return Ok(trades);
        }

        [NonAction]
        public async Task ValidateCreatedTrade(TraderAccount account, Commodity commodity, Trade trade)
        {

            //could make this return Iactionresult, and then check in main logic whether it's passed validation

            if (account is null)
            {
                await _auditLogService.CreateNewLogAsync(
                    "Trade", // Entity
                    "Create", // Change
                    trade.TraderId.ToString() + " which is a trade account of " + trade.Trader.User.Username, // Who changed
                    $"User {trade.Trader.User.Username} tried to create a trade of {trade.Quantity} of {trade.Commodity.CommodityName} on their account" +
                    $"{trade.Trader.AccountName} but failed due to a null account",
                    trade.Trader.User.Country.CountryName == "Russia"
                    );
                //return BadRequest("Failed to create trade due to a null account.");
            }

            if (commodity is null)
            {
                await _auditLogService.CreateNewLogAsync(
                "Trade", // Entity
                "Create", // Change
                trade.TraderId.ToString() + " which is a trade account of " + trade.Trader.User.Username, // Who changed
                $"User {trade.Trader.User.Username} tried to create a trade of {trade.Quantity} of {trade.Commodity.CommodityName} on their account" +
                $"{trade.Trader.AccountName} but failed due to a null commodity",
                trade.Trader.User.Country.CountryName == "Russia"
    );
                //return BadRequest("Failed to create trade due to a null commodity.");
            }

            if (trade.Quantity <= 0)
            {
                await _auditLogService.CreateNewLogAsync(
                    "Trade", // Entity
                    "Create", // Change
                    trade.TraderId.ToString() + " which is a trade account of " + trade.Trader.User.Username, // Who changed
                    $"User {trade.Trader.User.Username} tried to create a trade of {trade.Quantity} of {trade.Commodity.CommodityName} on their account" +
                    $"{trade.Trader.AccountName} but failed due to 0 or less quantity",
                    trade.Trader.User.Country.CountryName == "Russia"
                    );
                //return BadRequest("Failed to create trade due to a 0 quantity purchased.");
            }

            var priceOfCommodity = await _api.GetCommodityPrice(commodity.CommodityName);

            if ((account.Balance - priceOfCommodity * trade.Quantity) < 0)
            {
                await _auditLogService.CreateNewLogAsync(
                    "Trade", // Entity
                    "Create", // Change
                    trade.TraderId.ToString() + " which is a trade account of " + trade.Trader.User.Username, // Who changed
                    $"User {trade.Trader.User.Username} tried to create a trade of {trade.Quantity} of {trade.Commodity.CommodityName} on their account" +
                    $"{trade.Trader.AccountName} but failed due to a lack of funds in the account",
                    trade.Trader.User.Country.CountryName == "Russia"
                    );
                //return BadRequest("Failed to create trade due to account not having enough money for trade.");
            }
        }


    }
}
