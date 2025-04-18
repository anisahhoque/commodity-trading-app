﻿using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using CommodityTradingAPI.Models;
using Microsoft.AspNetCore.Hosting;
using System.Net.Http;
using System.Threading.Tasks;

using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CommodityTradingAPI.Data;
using System.Net.Http.Json;


namespace CommodityTradingAPITests
{
    [TestFixture]
    public class Tests
    {
        private CommoditiesDbContext _context;

        private HttpClient _client;


        // Setup an in-memory database for testing
        [SetUp]
        public void Setup()
        {

            var builder = new WebHostBuilder()
                .UseEnvironment("Testing");


            var options = new DbContextOptionsBuilder<CommoditiesDbContext>()
                .UseInMemoryDatabase(databaseName: "CommodityTradingTestDB")
                .Options;

            _context = new CommoditiesDbContext(options);

            // Ensure the database is created fresh for each test
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        // Tests for user-related functions
        // Test for creating a user
        [Test]
        public async Task TestCreateUser()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryId = 1
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users.FindAsync(user.UserId);

            Assert.That(createdUser, Is.Not.Null);  // Checks if the user was created
            Assert.That("testuser", Is.EqualTo(createdUser.Username));  // Verifies the username

        }

        // Test for deleting a user
        [Test]
        public async Task TestDeleteUser()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryId = 1
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var deletedUser = await _context.Users.FindAsync(user.UserId);

            Assert.That(deletedUser, Is.Null);  // Verifies that the user is deleted

        }

        // Test for editing a user
        [Test]
        public async Task TestEditUser()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryId = 1
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Username = "updateduser";
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var updatedUser = await _context.Users.FindAsync(user.UserId);

            Assert.That(updatedUser, Is.Not.Null);  // Verifies the user exists
            Assert.That("updateduser", Is.EqualTo(updatedUser.Username));  // Verifies the updated username

        }

        // Tests for trader-related functions
        // Test for creating a trader
        [Test]
        public async Task TestCreateTrader()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryId = 1
            };

            var trader = new TraderAccount
            {
                TraderId = Guid.NewGuid(),
                UserId = user.UserId,
                Balance = 100000,
                AccountName = "TestTrader"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.TraderAccounts.Add(trader);
            await _context.SaveChangesAsync();

            var createdTrader = await _context.TraderAccounts.FindAsync(trader.TraderId);

            Assert.That(createdTrader, Is.Not.Null);  // Verifies the trader is created
            Assert.That("TestTrader", Is.EqualTo(createdTrader.AccountName));  // Verifies the trader's account name

        }

        // Test for deleting a trader
        [Test]

        public async Task TestDeleteTrader()

        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryId = 1
            };

            var trader = new TraderAccount
            {
                TraderId = Guid.NewGuid(),
                UserId = user.UserId,
                Balance = 100000,
                AccountName = "TestTrader"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.TraderAccounts.Add(trader);
            await _context.SaveChangesAsync();

            _context.TraderAccounts.Remove(trader);
            await _context.SaveChangesAsync();


            var deletedTrader = await _context.TraderAccounts.FindAsync(trader.TraderId);
            Assert.That(deletedTrader, Is.Null);  // Verifies the trader is deleted

        }

        // Test for editing a trader
        [Test]

        public async Task TestEditTrader()

        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryId = 1
            };

            var trader = new TraderAccount
            {
                TraderId = Guid.NewGuid(),
                UserId = user.UserId,
                Balance = 100000,
                AccountName = "TestTrader"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.TraderAccounts.Add(trader);

            await _context.SaveChangesAsync();

            trader.AccountName = "UpdatedTrader";
            _context.TraderAccounts.Update(trader);
            await _context.SaveChangesAsync();

            var updatedTrader = await _context.TraderAccounts.FindAsync(trader.TraderId);
            Assert.That(updatedTrader, Is.Not.Null);  // Verifies trader exists
            Assert.That("UpdatedTrader", Is.EqualTo(updatedTrader.AccountName));  // Verifies the account name is updated
        }

        [Test]
        public async Task TestCreateUser_InvalidCountry()
        {
            var newUserDetails = new CreateUser
            {
                Username = "newuser",
                PasswordRaw = "password123",
                Country = "InvalidCountry"
            };

            var response = await _client.PostAsJsonAsync("/api/User", newUserDetails);
            Assert.That(HttpStatusCode.BadRequest, Is.EqualTo(response.StatusCode));
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.That(responseContent.Contains("Invalid country name."),Is.True);
        }

        [Test]
        public async Task TestCreateUser_DuplicateUsername()
        {
            var existingUser = new User
            {
                UserId = Guid.NewGuid(),
                Username = "existinguser",
                PasswordHash = "hashedpassword",
                CountryId = 1
            };
            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var newUserDetails = new CreateUser
            {
                Username = "existinguser",
                PasswordRaw = "newpassword123",
                Country = "ValidCountry"
            };

            var response = await _client.PostAsJsonAsync("/api/User", newUserDetails);
            Assert.That(HttpStatusCode.BadRequest, Is.EqualTo(response.StatusCode));
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.That(responseContent.Contains("Username already exists."), Is.True);
        }

        [Test]
        public async Task TestUnauthorizedUserAccess()
        {
            // Make an unauthenticated request to the Index endpoint
            var response = await _client.GetAsync("/api/User");
            Assert.That(HttpStatusCode.Unauthorized, Is.EqualTo(response.StatusCode));
        }

        [Test]
        public async Task TestDeleteUser_WithRelatedTrader()
        {
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryId = 1
            };

            var trader = new TraderAccount
            {
                TraderId = Guid.NewGuid(),
                UserId = user.UserId,
                Balance = 100000,
                AccountName = "TestTrader"
            };

            _context.Users.Add(user);
            _context.TraderAccounts.Add(trader);
            await _context.SaveChangesAsync();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var deletedUser = await _context.Users.FindAsync(user.UserId);
            var deletedTrader = await _context.TraderAccounts.FindAsync(trader.TraderId);

            Assert.That(deletedUser, Is.Null);  // Verifies the user was deleted
            Assert.That(deletedTrader, Is.Null);  // Verifies the trader was deleted (if cascade delete is enabled)
        }

        [Test]
        public async Task TestGetUserById_UserNotFound()
        {
            var nonExistentUserId = Guid.NewGuid();  // Use a GUID that is not in the database
            var response = await _client.GetAsync($"/api/User/{nonExistentUserId}");
            Assert.That(HttpStatusCode.NotFound, Is.EqualTo(response.StatusCode));

        }

        // Clean up after testing
        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _context.Dispose();
        }
    }
}
