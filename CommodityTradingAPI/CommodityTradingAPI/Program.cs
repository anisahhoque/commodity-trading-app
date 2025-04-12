using Microsoft.AspNetCore.Authentication.JwtBearer;
using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CommodityTradingAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<CommoditiesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")
    ?? ("Connection string 'DefaultConnection' not found."))
    .UseSeeding((context, _) =>
     {
         // Check if Roles table is empty
         var roles = context.Set<Role>();

         if (!roles.Any())
         {
             var managerRole = new Role { RoleId = Guid.NewGuid(), RoleName = "Manager" };
             var traderRole = new Role { RoleId = Guid.NewGuid(), RoleName = "Trader" };

             roles.AddRange(managerRole, traderRole);
             context.SaveChanges();
         }
         // Check if the Admin user exists
         var AdminExists = context.Set<User>().Any(u => u.Username == "Admin");

         if (!AdminExists)
         {
             context.Set<User>().Add(new User
             {
                 Username = "Admin",
                 PasswordHash = BCrypt.Net.BCrypt.HashPassword(builder.Configuration["AdminPassword"]),
                 CountryId = 14
             });
             context.SaveChanges();
             var adminId = context.Set<User>().FirstOrDefault(u => u.Username == "Admin").UserId;
             var manager = context.Set<Role>().FirstOrDefault(r => r.RoleName == "Manager");

             context.Set<RoleAssignment>().Add(new RoleAssignment
             {
                 AssignmentId = Guid.NewGuid(),
                 UserId = adminId,
                 RoleId = manager.RoleId
             });

             context.SaveChanges();
         }
     }
    ));

builder.Services.AddHttpClient<ExternalApiService>();

builder.Services.AddAuthorization();

builder.Services.AddSingleton<CommodityTradingAPI.Services.ILogger, AuditLogService>();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var CommoditiesDbContext = scope.ServiceProvider.GetRequiredService<CommoditiesDbContext>();
    CommoditiesDbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
