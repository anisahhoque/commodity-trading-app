using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using CommodityTradingAPI.Data;
using CommodityTradingAPI.Models;


namespace CommodityTradingAPITests
{
    public class Tests
    {
        private CommoditiesDbContext _context;

        // Setup an in-memory database for testing
        [SetUp]
        public void Setup()
        {
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
            Assert.IsNotNull(createdUser);
            Assert.AreEqual("testuser", createdUser.Username);
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
            Assert.IsNull(deletedUser);
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
            Assert.IsNotNull(updatedUser);
            Assert.AreEqual("updateduser", updatedUser.Username);
        }

        // Tests for trader-related functions
        // Test for creating a user
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
            Assert.IsNotNull(createdTrader);
            Assert.AreEqual("TestTrader", createdTrader.AccountName);
        }

        // Test for deleting a user
        [Test]
        public async Task TestDeleteUser2()
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

            var createdTrader = await _context.TraderAccounts.FindAsync(trader.TraderId);
            Assert.IsNull(createdTrader);
        }

        // Test for editing a user
        [Test]
        public async Task TestEditUser2()
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

            trader.AccountName = "updatedtrader";
            _context.TraderAccounts.Update(trader);
            await _context.SaveChangesAsync();

            var updatedTrader = await _context.TraderAccounts.FindAsync(trader.TraderId);
            Assert.IsNotNull(updatedTrader);
            Assert.AreEqual("updatedtrader", updatedTrader.AccountName);
        }

        // Clean up after testing
        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
    }
}
