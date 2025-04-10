using CommodityTradingAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CommodityTradingAPI.Data;

public partial class CommoditiesDbContext : DbContext
{
    public CommoditiesDbContext()
    {
    }

    public CommoditiesDbContext(DbContextOptions<CommoditiesDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Commodity> Commodities { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RoleAssignment> RoleAssignments { get; set; }

    public virtual DbSet<Trade> Trades { get; set; }

    public virtual DbSet<TradeMitigation> TradeMitigations { get; set; }

    public virtual DbSet<TraderAccount> TraderAccounts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwTraderTrade> VwTraderTrades { get; set; }

    public virtual DbSet<VwTradesByCommodity> VwTradesByCommodities { get; set; }

    public virtual DbSet<VwUserRole> VwUserRoles { get; set; }

    public virtual DbSet<VwUserTradingAccount> VwUserTradingAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Default");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Commodity>(entity =>
        {
            entity.HasKey(e => e.CommodityId).HasName("commodity_commodityid_primary");

            entity.ToTable("commodity");

            entity.Property(e => e.CommodityId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("CommodityID");
            entity.Property(e => e.CommodityName).HasMaxLength(50);
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.CountryId).HasName("country_countryid_primary");

            entity.ToTable("country");

            entity.Property(e => e.CountryId)
                .ValueGeneratedOnAdd()
                .HasColumnName("CountryID");
            entity.Property(e => e.CountryName).HasMaxLength(50);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("role_roleid_primary");

            entity.ToTable("role");

            entity.Property(e => e.RoleId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(15);
        });

        modelBuilder.Entity<RoleAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("role_assignment_assignmentid_primary");

            entity.ToTable("role_assignment");

            entity.Property(e => e.AssignmentId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("AssignmentID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleAssignments)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_assignment_roleid_foreign");

            entity.HasOne(d => d.User).WithMany(p => p.RoleAssignments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("role_assignment_userid_foreign");
        });

        modelBuilder.Entity<Trade>(entity =>
        {
            entity.HasKey(e => e.TradeId).HasName("trade_tradeid_primary");

            entity.ToTable("trade");

            entity.Property(e => e.TradeId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("TradeID");
            entity.Property(e => e.Bourse).HasMaxLength(10);
            entity.Property(e => e.CommodityId).HasColumnName("CommodityID");
            entity.Property(e => e.Contract).HasMaxLength(5);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Expiry).HasColumnType("datetime");
            entity.Property(e => e.MitigationId).HasColumnName("MitigationID");
            entity.Property(e => e.PricePerUnit).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TraderId).HasColumnName("TraderID");

            entity.HasOne(d => d.Commodity).WithMany(p => p.Trades)
                .HasForeignKey(d => d.CommodityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trade_commodityid_foreign");

            entity.HasOne(d => d.Mitigation).WithMany(p => p.Trades)
                .HasForeignKey(d => d.MitigationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trade_mitigationid_foreign");

            entity.HasOne(d => d.Trader).WithMany(p => p.Trades)
                .HasForeignKey(d => d.TraderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trade_traderid_foreign");
        });

        modelBuilder.Entity<TradeMitigation>(entity =>
        {
            entity.HasKey(e => e.MitigationId).HasName("trade_mitigations_mitigationid_primary");

            entity.ToTable("trade_mitigations");

            entity.Property(e => e.MitigationId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("MitigationID");
            entity.Property(e => e.SellPointLoss).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SellPointProfit).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<TraderAccount>(entity =>
        {
            entity.HasKey(e => e.TraderId).HasName("trader_account_traderid_primary");

            entity.ToTable("trader_account");

            entity.Property(e => e.TraderId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("TraderID");
            entity.Property(e => e.AccountName).HasMaxLength(50);
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.TraderAccounts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("trader_account_userid_foreign");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("user_userid_primary");

            entity.ToTable("user");

            entity.Property(e => e.UserId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UserID");
            entity.Property(e => e.CountryId).HasColumnName("CountryID");
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Country).WithMany(p => p.Users)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_countryid_foreign");
        });

        modelBuilder.Entity<VwTraderTrade>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_trader_trades");

            entity.Property(e => e.Bourse).HasMaxLength(10);
            entity.Property(e => e.CommodityName).HasMaxLength(50);
            entity.Property(e => e.Contract).HasMaxLength(5);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Expiry).HasColumnType("datetime");
            entity.Property(e => e.PricePerUnit).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SellPointLoss).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.SellPointProfit).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TradeId).HasColumnName("TradeID");
            entity.Property(e => e.TraderId).HasColumnName("TraderID");
        });

        modelBuilder.Entity<VwTradesByCommodity>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_trades_by_commodity");

            entity.Property(e => e.CommodityName).HasMaxLength(50);
            entity.Property(e => e.TotalBuyValue).HasColumnType("decimal(38, 0)");
            entity.Property(e => e.TotalSellValue).HasColumnType("decimal(38, 0)");
        });

        modelBuilder.Entity<VwUserRole>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_user_roles");

            entity.Property(e => e.RoleName).HasMaxLength(15);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<VwUserTradingAccount>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_user_trading_accounts");

            entity.Property(e => e.AccountName).HasMaxLength(50);
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Bourse).HasMaxLength(10);
            entity.Property(e => e.CommodityName).HasMaxLength(50);
            entity.Property(e => e.Contract).HasMaxLength(5);
            entity.Property(e => e.CountryName).HasMaxLength(50);
            entity.Property(e => e.Expiry).HasColumnType("datetime");
            entity.Property(e => e.PricePerUnit).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.TradeCreatedAt).HasColumnType("datetime");
            entity.Property(e => e.TradeId).HasColumnName("TradeID");
            entity.Property(e => e.TraderId).HasColumnName("TraderID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
