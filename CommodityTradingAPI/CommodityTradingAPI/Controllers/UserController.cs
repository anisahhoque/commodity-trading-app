﻿using CommodityTradingAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using CommodityTradingAPI.Models;
using System.Data;
//using ILogger = CommodityTradingAPI.Services.ILogger;

namespace CommodityTradingAPI.Controllers
{
    [Route("")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly CommoditiesDbContext _context;
        private CommodityTradingAPI.Services.ILogger _auditLogService;

        public UserController(CommoditiesDbContext context, CommodityTradingAPI.Services.ILogger logger)
        {
            _context = context;
            _auditLogService = logger;
        }

        [HttpGet]
        //[Authorize(Roles = "Manager")]
        public async Task<string> Index()
        {
            var users = await _context.Users
                    .Include(static u => u.Country)
                    .Include(u => u.TraderAccounts)
                    .Include(u => u.RoleAssignments)
                    
                    .ThenInclude(ra => ra.Role).ToListAsync();
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(users, settings);
        }

        // No need to authorise as anyone should be able to make a new account
        [HttpPost("Create")]
        public async Task<IActionResult> Create([Bind("Username", "PasswordRaw", "Country", "Role")] CreateUser newUserDetails)
        {

            // Try to find the matching country
            var matchedCountry = await _context.Countries
                .FirstOrDefaultAsync(c => c.CountryName!.ToLower() == newUserDetails.Country.ToLower());

            var role = newUserDetails.Role.ToLower().Trim();
            if (role != "manager" && role != "trader")
                return BadRequest("Invalid role. Must be either 'Manager' or 'Trader'.");

            Guid RoleId = _context.Roles.First(r => r.RoleName.ToLower() == role).RoleId;

            if (matchedCountry == null)
            {
                return BadRequest(new { error = "Invalid country name." });
            }

            User newUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = newUserDetails.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUserDetails.PasswordRaw),
                CountryId = matchedCountry.CountryId

            };

            RoleAssignment roleAssignment = new RoleAssignment
            {
                AssignmentId = Guid.NewGuid(),
                UserId = newUser.UserId,
                RoleId = RoleId
            };

            _context.Add(newUser);
            _context.Add(roleAssignment);
            await _context.SaveChangesAsync();

            var response = new UserResponse
            {
                UserId = newUser.UserId,
                Username = newUser.Username,
                CountryName = newUser.Country.CountryName!,
                Roles = newUser.RoleAssignments.Select(ra => ra.Role.RoleName).ToList()
            };

            await _auditLogService.CreateNewLogAsync(
                "User", // Entity
                "Create", // Change
                newUser.Username,
                $"New user created",
                newUser.Country.CountryName == "Russia"
                );

            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UserId }, response);
        }

        // Get a user from ID
        [HttpGet("{id}")]
        public async Task<string> GetUserById(Guid id)
        {
            var user = await _context.Users
                .Include(u => u.Country)
                .Include(u => u.RoleAssignments)
                    .ThenInclude(ra => ra.Role)
                .Include(u => u.TraderAccounts)
                .ThenInclude(ta => ta.Trades)
                .FirstOrDefaultAsync(u => u.UserId == id);



            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(user, settings);
            
        }
        //suspend?
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] bool confirm = false)
        {
            var user = await _context.Users
                .Include(u => u.RoleAssignments)
                .Include(u => u.TraderAccounts)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound("User not found.");

            if (user.TraderAccounts.Any() && !confirm)
            {
                return BadRequest("User has active trader accounts. Confirm deletion by adding '?confirm=true'.");
            }

            await _auditLogService.CreateNewLogAsync(
            "User", // Entity
            "Delete", // Change
            user.Username,
            $"User {user.Username} deleted",
            user.Country.CountryName == "Russia"
            );

            _context.RoleAssignments.RemoveRange(user.RoleAssignments);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] EditUser model)
        {
            if (id != model.UserId)
            {
                return BadRequest("User ID mismatch.");
            }

            var user = await _context.Users
                .Include(u => u.RoleAssignments)
                .Include(u => u.Country)  
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            
            if (!string.IsNullOrEmpty(model.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            
            if (model.CountryId != user.CountryId)
            {
                user.CountryId = model.CountryId;
            }

            
            _context.RoleAssignments.RemoveRange(user.RoleAssignments);
            foreach (var roleId in model.SelectedRoleIds)
            {
                _context.RoleAssignments.Add(new RoleAssignment
                {
                    UserId = user.UserId,
                    RoleId = roleId
                });
            }

            await _context.SaveChangesAsync();

            await _auditLogService.CreateNewLogAsync(
            "User", // Entity
            "Update", // Change
            user.Username,
            $"User {user.Username} updated",
            user.Country.CountryName == "Russia"
            );

            return Ok(new { message = "User updated successfully." });
        }

        public class UserResponse // DTO, basically a response object to put in the body
        {
            public Guid UserId { get; set; }
            public string Username { get; set; }
            public string CountryName { get; set; }
            public List<string> Roles { get; set; } = new();
        }
    }

}