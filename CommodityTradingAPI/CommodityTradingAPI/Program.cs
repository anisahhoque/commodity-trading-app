using Microsoft.AspNetCore.Authentication.JwtBearer;
using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<CommoditiesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")
    ?? ("Connection string 'DefaultConnection' not found."))
    .UseSeeding((context, _) =>
     {
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
         }
     }
    ));


builder.Services.AddAuthorization();


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
