using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace CommodityTradingAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<CommoditiesDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."))
                    .UseSeeding((context, _) =>
                    {
                        var adminExists = context.Set<User>().Any(u => u.Username == "Admin");

                        if (!adminExists)
                        {
                            context.Set<User>().Add(new User
                            {
                                Username = "Admin",
                                PasswordHash = BCrypt.Net.BCrypt.HashPassword(builder.Configuration["AdminPassword"]),
                                CountryId = 14 // Example CountryId, make sure this matches an actual country in your database
                            });
                            context.SaveChanges();
                        }
                    })
            );

            // Add authentication services
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // Change this to `true` in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                    ValidAudience = builder.Configuration["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Cookies["AuthToken"];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();

            // Add OpenAPI/Swagger services
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Seed the database
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CommoditiesDbContext>();
                dbContext.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
