using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using CommodityTradingAPI.Models.DTOs;
using CommodityTradingAPI.Services;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

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
        public async Task<string> Index()
        {
            var traders = await _context.TraderAccounts
                .Include(t => t.Trades)
                .Include(u => u.User).ToListAsync();

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(traders, settings);
        }



        [HttpGet("{id}")]
        public async Task<string> Details(Guid id)

        //returns the trading accounts balance, plus all the traders' currently open trades
        {
            var trader = await _context.TraderAccounts
                            .Include(t => t.User)
                            .FirstOrDefaultAsync(t => t.TraderId == id);

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };


            var user = trader.User;
            var trades = await _context.Trades.Where(t => t.TraderId == id && t.IsOpen).ToListAsync();


            TraderAccountPortfolioDto traderPortfolio = new()
            {
                AccountName = trader.AccountName,
                UserName = user.Username,
                Balance = trader.Balance,
                OpenAccountTrades = trades
            };

            return JsonConvert.SerializeObject(traderPortfolio, settings);

        }

        //should we also have an endpoint just to get a trading accounts balance??? maybe not needed
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<List<TraderAccount>>> GetAllUserTraderAccounts(Guid userId)
        {
            //Get all accoutns associated with a specific user
            var userAccounts = await _context.TraderAccounts.Where(x => x.UserId == userId)
                .Include(x => x.User).ToListAsync();
            return userAccounts;
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("UserId","Balance","AccountName")] CreateTraderAccount model)
        {
            if (model == null)
            {
                return BadRequest("Invalid trader account");
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);
            if (user == null)
            {
                return BadRequest("User not found");
            }
            var traderAccount = new TraderAccount
            {
                TraderId = Guid.NewGuid(), 
                UserId = model.UserId,
                Balance = model.Balance,
                AccountName = model.AccountName
            };
            user.TraderAccounts.Add(traderAccount);
            _context.TraderAccounts.Add(traderAccount);
            await _context.SaveChangesAsync();
            return Ok(traderAccount);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] EditTraderAccount model)
        {
            if (id != model.TraderId)
            {
                return BadRequest("Trader Id mismatch.");
            }
            var trader = await _context.TraderAccounts
                                        .FirstOrDefaultAsync(t => t.TraderId == id);
            if (!string.IsNullOrEmpty(model.AccountName))
            {
                trader.AccountName = model.AccountName;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Trader updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var trader = await _context.TraderAccounts
                          .Include(t => t.User)
                          .Include(t => t.Trades)
                          .FirstOrDefaultAsync(t => t.TraderId == id);
            if (trader == null)
            {
                return NotFound("Trader not found");
            }
            _context.TraderAccounts.Remove(trader);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("Deposit/{id}")]
        public async Task<IActionResult> Deposit([Bind("TraderId","Amount")] DepositDto deposit)
        {
            if (deposit == null || deposit.Amount <= 0)
            {
                return BadRequest("Invalid deposit request.");
            }

            var trader = await _context.TraderAccounts.FindAsync(deposit.TraderId);
            if (trader == null)
            {
                return NotFound("Trader not found.");
            }

            trader.Balance += deposit.Amount;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Deposit successful",
                NewBalance = trader.Balance
            });
        }
        [HttpPost("Withdraw/{id}")]
        public async Task<IActionResult> Withdraw([Bind("TraderId", "Amount")] DepositDto withdraw)
        {
            if (withdraw == null || withdraw.Amount <= 0)
            {
                return BadRequest("Invalid withdrawal request.");
            }

            var trader = await _context.TraderAccounts.FindAsync(withdraw.TraderId);
            if (trader == null)
            {
                return NotFound("Trader not found.");
            }

            if (trader.Balance < withdraw.Amount)
            {
                return BadRequest("Insufficient balance.");
            }

            trader.Balance -= withdraw.Amount;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Withdrawal successful",
                NewBalance = trader.Balance
            });
        }


    }
}
