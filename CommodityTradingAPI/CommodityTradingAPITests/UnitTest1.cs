using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CommodityTradingAPITests
{
    public class Tests
    {
        private ApplicationDbContext _context;

        // Setup an in-memory database for testing
        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CommodityTradingTestDB")
                .Options;

            _context = new ApplicationDbContext(options);

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
                UserID = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryID = 1
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var createdUser = await _context.Users.FindAsync(user.UserID);
            Assert.IsNotNull(createdUser);
            Assert.AreEqual("testuser", createdUser.Username);
        }

        // Test for deleting a user
        [Test]
        public async Task TestDeleteUser()
        {
            var user = new User
            {
                UserID = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryID = 1
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            var deletedUser = await _context.Users.FindAsync(user.UserID);
            Assert.IsNull(deletedUser);
        }

        // Test for editing a user
        [Test]
        public async Task TestEditUser()
        {
            var user = new User
            {
                UserID = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryID = 1
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.Username = "updateduser";
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var updatedUser = await _context.Users.FindAsync(user.UserID);
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
                UserID = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryID = 1
            };

            var trader = new Trader
            {
                TraderID = Guid.NewGuid(),
                UserID = user.UserID,
                Balance = 100000,
                AccountName = "TestTrader"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.Traders.Add(trader);
            await _context.SaveChangesAsync();

            var createdTrader = await _context.Traders.FindAsync(trader.TraderID);
            Assert.IsNotNull(createdTrader);
            Assert.AreEqual("TestTrader", createdTrader.AccountName);
        }

        // Test for deleting a user
        [Test]
        public async Task TestDeleteUser()
        {
            var user = new User
            {
                UserID = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryID = 1
            };

            var trader = new Trader
            {
                TraderID = Guid.NewGuid(),
                UserID = user.UserID,
                Balance = 100000,
                AccountName = "TestTrader"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.Traders.Add(trader);
            await _context.SaveChangesAsync();

            _context.Traders.Remove(trader);
            await _context.SaveChangesAsync();

            var createdTrader = await _context.Traders.FindAsync(trader.TraderID);
            Assert.IsNull(createdTrader);
        }

        // Test for editing a user
        [Test]
        public async Task TestEditUser()
        {
            var user = new User
            {
                UserID = Guid.NewGuid(),
                Username = "testuser",
                PasswordHash = "hashedpassword",
                CountryID = 1
            };

            var trader = new Trader
            {
                TraderID = Guid.NewGuid(),
                UserID = user.UserID,
                Balance = 100000,
                AccountName = "TestTrader"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _context.Traders.Add(trader);
            await _context.SaveChangesAsync();

            trader.AccountName = "updatedtrader";
            _context.Trader.Update(trader);
            await _context.SaveChangesAsync();

            var updatedTrader = await _context.Traders.FindAsync(trader.TraderID);
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
