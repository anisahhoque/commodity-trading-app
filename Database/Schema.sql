-- Drop views if they exist before recreating them
DROP VIEW IF EXISTS vw_user_trading_accounts;
DROP VIEW IF EXISTS vw_trades_by_commodity;
DROP VIEW IF EXISTS vw_user_roles;
DROP VIEW IF EXISTS vw_trader_trades;

GO

	
	
-- Drop tables if they exist before recreating them
DROP TABLE IF EXISTS "user";
DROP TABLE IF EXISTS "role";
DROP TABLE IF EXISTS "commodity";
DROP TABLE IF EXISTS "trade_mitigations";
DROP TABLE IF EXISTS "role_assignment";
DROP TABLE IF EXISTS "trader_account";
DROP TABLE IF EXISTS "trade";

GO


	
--- Create tables with no foreign keys first
-- Creating user table
CREATE TABLE "user"(
	"UserID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"Username" NVARCHAR(50) NOT NULL,
	"PasswordHash" NVARCHAR(255) NOT NULL
);
ALTER TABLE
	"user" ADD CONSTRAINT "user_userid_primary" PRIMARY KEY("UserID");


-- Create role table
CREATE TABLE "role"(
	"RoleID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"RoleName" NVARCHAR(15) NOT NULL
);
ALTER TABLE
	"role" ADD CONSTRAINT "role_roleid_primary" PRIMARY KEY("RoleID");


-- Creating commodity table
CREATE TABLE "commodity"(
	"CommodityID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"CommodityName" NVARCHAR(50) NOT NULL
);
ALTER TABLE
	"commodity" ADD CONSTRAINT "commodity_commodityid_primary" PRIMARY KEY("CommodityID");


 -- Creating trade_mitigations
CREATE TABLE "trade_mitigations"(
	"MitigationID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"SellPointProfit" BIGINT,
	"SellPointLoss" BIGINT
);
ALTER TABLE
	"trade_mitigations" ADD CONSTRAINT "trade_mitigations_mitigationid_primary" PRIMARY KEY("MitigationID");



--- Create tables with foriegn keys

-- Creating role_assignment table
CREATE TABLE "role_assignment"(
	"AssignmentID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"UserID" INT NOT NULL,
	"RoleID" TINYINT NOT NULL
);
ALTER TABLE
	"role_assignment" ADD CONSTRAINT "role_assignment_assignmentid_primary" PRIMARY KEY("AssignmentID");
ALTER TABLE
	"role_assignment" ADD CONSTRAINT "role_assignment_userid_foreign" FOREIGN KEY("UserID") REFERENCES "user"("UserID");
ALTER TABLE
	"role_assignment" ADD CONSTRAINT "role_assignment_roleid_foreign" FOREIGN KEY("RoleID") REFERENCES "role"("RoleID");


 -- Create trader_account table
CREATE TABLE "trader_account"(
	"TraderID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"UserID" INT NOT NULL,
	"Balance" BIGINT NOT NULL,
	"AccountName" NVARCHAR(50) NOT NULL
);
ALTER TABLE
	"trader_account" ADD CONSTRAINT "trader_account_traderid_primary" PRIMARY KEY("TraderID");
ALTER TABLE
	"trader_account" ADD CONSTRAINT "trader_account_userid_foreign" FOREIGN KEY("UserID") REFERENCES "user"("UserID");


 -- Creating trade table
CREATE TABLE "trade"(
	"TradeID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"TraderID" INT NOT NULL,
	"CommodityID" TINYINT NOT NULL,
	"PricePerUnit" BIGINT NOT NULL,
	"Quantity" TINYINT NOT NULL,
	"IsBuy" BIT NOT NULL,
	"Expiry" DATETIME NOT NULL,
	"CreatedAt" DATETIME NOT NULL,
	"Bourse" NVARCHAR(10) NOT NULL,
	"MitigationID" BIGINT NOT NULL,
	"IsOpen" BIT NOT NULL,
	"Contract" NVARCHAR(5) NOT NULL
);
ALTER TABLE
	"trade" ADD CONSTRAINT "trade_tradeid_primary" PRIMARY KEY("TradeID");
ALTER TABLE
	"trade" ADD CONSTRAINT "trade_mitigationid_foreign" FOREIGN KEY("MitigationID") REFERENCES "trade_mitigations"("MitigationID");
ALTER TABLE
	"trade" ADD CONSTRAINT "trade_traderid_foreign" FOREIGN KEY("TraderID") REFERENCES "trader_account"("TraderID");
ALTER TABLE
	"trade" ADD CONSTRAINT "trade_commodityid_foreign" FOREIGN KEY("CommodityID") REFERENCES "commodity"("CommodityID");
GO

--- Add views
-- View for all the trades by trader, use the command SELECT * FROM vw_trader_trades WHERE "TraderID" = [TraderID]; to get history.
-- Use SELECT * FROM vw_trader_trades WHERE "TraderID" = [TraderID] AND IsOpen = 1; to get current open positions
CREATE VIEW vw_trader_trades AS
SELECT
	t."TradeID",
	t."TraderID",
	c."CommodityName",
	t."PricePerUnit",
	t."Quantity",
	t."IsBuy",
	t."Expiry",
	t."CreatedAt",
	t."Bourse",
	tm."SellPointProfit",
	tm."SellPointLoss",
	t."IsOpen",
	t."Contract"
FROM
	"trade" t
JOIN
	"commodity" c ON t."CommodityID" = c."CommodityID"
JOIN
	"trade_mitigations" tm ON t."MitigationID" = tm."MitigationID";
GO


-- View for all roles of a certain user, use SELECT * FROM vw_user_roles WHERE "UserID" = [UserID];
CREATE VIEW vw_user_roles AS
SELECT
	u."UserID",
	u."Username",
	r."RoleName"
FROM
	"user" u
JOIN
	"role_assignment" ra ON u."UserID" = ra."UserID"
JOIN
	"role" r ON ra."RoleID" = r."RoleID";
GO


-- View for what commodities are popular use SELECT * FROM vw_trades_by_commodity; to see full breakdown
CREATE VIEW vw_trades_by_commodity AS
SELECT
	c."CommodityName",
	SUM(CASE WHEN t."IsBuy" = 1 THEN t."Quantity" ELSE 0 END) AS TotalBuyQuantity,
	SUM(CASE WHEN t."IsBuy" = 1 THEN t."PricePerUnit" * t."Quantity" ELSE 0 END) AS TotalBuyValue,
	SUM(CASE WHEN t."IsBuy" = 0 THEN t."Quantity" ELSE 0 END) AS TotalSellQuantity,
	SUM(CASE WHEN t."IsBuy" = 0 THEN t."PricePerUnit" * t."Quantity" ELSE 0 END) AS TotalSellValue
FROM
	"trade" t
JOIN
	"commodity" c ON t."CommodityID" = c."CommodityID"
GROUP BY
	c."CommodityName";
GO


-- View for details on all trading accounts of a particular user use SELECT * FROM vw_user_trading_accounts WHERE "UserID" = [UserID]; to see a single users details
CREATE VIEW vw_user_trading_accounts AS
SELECT
	u."UserID",
	u."Username",
	ta."TraderID",
	ta."AccountName",
	ta."Balance",
	c."CommodityName",
	t."TradeID",
	t."PricePerUnit",
	t."Quantity",
	t."IsBuy",
	t."Expiry",
	t."CreatedAt" AS TradeCreatedAt,
	t."IsOpen",
	t."Bourse",
	t."Contract"
FROM
	"user" u
JOIN
	"trader_account" ta ON u."UserID" = ta."UserID"
LEFT JOIN
	"trade" t ON ta."TraderID" = t."TraderID"
LEFT JOIN
	"commodity" c ON t."CommodityID" = c."CommodityID";
GO
