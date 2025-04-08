--- Create tables with no foreign keys first

-- Creating user table
CREATE TABLE "user"(
    "UserID" INT IDENTITY(1,1) NOT NULL,
    "Username" NVARCHAR(50) NOT NULL,
    "PasswordHash" NVARCHAR(255) NOT NULL
);
ALTER TABLE
    "user" ADD CONSTRAINT "user_userid_primary" PRIMARY KEY("UserID");


-- Create role table
CREATE TABLE "role"(
    "RoleID" TINYINT IDENTITY(1,1) NOT NULL,
    "RoleName" NVARCHAR(15) NOT NULL
);
ALTER TABLE
    "role" ADD CONSTRAINT "role_roleid_primary" PRIMARY KEY("RoleID");


-- Creating commodity table
CREATE TABLE "commodity"(
    "CommodityID" TINYINT IDENTITY(1,1) NOT NULL,
    "CommodityName" NVARCHAR(50) NOT NULL
);
ALTER TABLE
    "commodity" ADD CONSTRAINT "commodity_commodityid_primary" PRIMARY KEY("CommodityID");


 -- Creating trade_mitigations
CREATE TABLE "trade_mitigations"(
    "MitigationID" BIGINT IDENTITY(1,1) NOT NULL,
    "SellPointProfit" BIGINT NOT NULL,
    "SellPointLoss" BIGINT NOT NULL
);
ALTER TABLE
    "trade_mitigations" ADD CONSTRAINT "trade_mitigations_mitigationid_primary" PRIMARY KEY("MitigationID");



--- Create tables with foriegn keys

-- Creating role_assignment table
CREATE TABLE "role_assignment"(
    "AssignmentID" INT IDENTITY(1,1) NOT NULL,
    "UserID" INT NOT NULL,
    "RoleID" TINYINT NOT NULL
);
ALTER TABLE
    "role_assignment" ADD CONSTRAINT "role_assignment_assignmentid_primary" PRIMARY KEY("AssignmentID");
ALTER TABLE
    "role_assignment" ADD CONSTRAINT "role_assignment_userid_foreign" FOREIGN KEY("UserID") REFERENCES "user"("UserID");
ALTER TABLE
    "role_assignment" ADD CONSTRAINT "role_assignment_roleid_foreign" FOREIGN KEY("RoleID") REFERENCES "role"("RoleID");


 -- Create trader_detail table
CREATE TABLE "trader_detail"(
    "TraderID" INT IDENTITY(1,1) NOT NULL,
    "UserID" INT NOT NULL,
    "Balance" BIGINT NOT NULL
);
ALTER TABLE
    "trader_detail" ADD CONSTRAINT "trader_detail_traderid_primary" PRIMARY KEY("TraderID");
ALTER TABLE
    "trader_detail" ADD CONSTRAINT "trader_detail_userid_foreign" FOREIGN KEY("UserID") REFERENCES "user"("UserID");


 -- Creating trade table
CREATE TABLE "trade"(
    "TradeID" BIGINT IDENTITY(1,1) NOT NULL,
    "TraderID" INT NOT NULL,
    "CommodityID" TINYINT NOT NULL,
    "PricePerUnit" BIGINT NOT NULL,
    "Quantity" TINYINT NOT NULL,
    "IsBuy" BIT NOT NULL,
    "Expiry" DATETIME NOT NULL,
    "CreatedAt" DATETIME NOT NULL,
    "Bourse" NVARCHAR(10) NOT NULL,
    "MitigationID" BIGINT NOT NULL,
    "IsOpen" BIT NOT NULL
);
ALTER TABLE
    "trade" ADD CONSTRAINT "trade_tradeid_primary" PRIMARY KEY("TradeID");
ALTER TABLE
    "trade" ADD CONSTRAINT "trade_mitigationid_foreign" FOREIGN KEY("MitigationID") REFERENCES "trade_mitigations"("MitigationID");
ALTER TABLE
    "trade" ADD CONSTRAINT "trade_traderid_foreign" FOREIGN KEY("TraderID") REFERENCES "trader_detail"("TraderID");
ALTER TABLE
    "trade" ADD CONSTRAINT "trade_commodityid_foreign" FOREIGN KEY("CommodityID") REFERENCES "commodity"("CommodityID");
