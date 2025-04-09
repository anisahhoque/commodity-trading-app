-- Drop views if they exist before recreating them
DROP VIEW IF EXISTS vw_user_trading_accounts;
DROP VIEW IF EXISTS vw_trades_by_commodity;
DROP VIEW IF EXISTS vw_user_roles;
DROP VIEW IF EXISTS vw_trader_trades;
GO

-- Drop tables if they exist before recreating them
DROP TABLE IF EXISTS "role";
DROP TABLE IF EXISTS "commodity";
DROP TABLE IF EXISTS "trade_mitigations";
DROP TABLE IF EXISTS "role_assignment";
DROP TABLE IF EXISTS "trader_account";
DROP TABLE IF EXISTS "trade";
DROP TABLE IF EXISTS "country";
DROP TABLE IF EXISTS "user";
GO

--- Create tables with no foreign keys first
-- Creating country table
CREATE TABLE "country"(
	"CountryID" TINYINT IDENTITY(1,1) NOT NULL,
	"Country" NVARCHAR(50)
);
ALTER TABLE
    "country" ADD CONSTRAINT "country_countryid_primary" PRIMARY KEY("CountryID")

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
	"SellPointProfit" DECIMAL,
	"SellPointLoss" DECIMAL
);
ALTER TABLE
	"trade_mitigations" ADD CONSTRAINT "trade_mitigations_mitigationid_primary" PRIMARY KEY("MitigationID");

--- Create tables with foreign keys
-- Creating user table
CREATE TABLE "user"(
	"UserID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"Username" NVARCHAR(50) NOT NULL,
	"PasswordHash" NVARCHAR(255) NOT NULL,
	"CountryID" TINYINT NOT NULL
);
ALTER TABLE
	"user" ADD CONSTRAINT "user_userid_primary" PRIMARY KEY("UserID");
ALTER TABLE
	"user" ADD CONSTRAINT "user_countryid_foreign" FOREIGN KEY("CountryID") REFERENCES "country"("CountryID");

-- Creating role_assignment table
CREATE TABLE "role_assignment"(
	"AssignmentID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"UserID" UNIQUEIDENTIFIER NOT NULL,
	"RoleID" UNIQUEIDENTIFIER NOT NULL
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
	"UserID" UNIQUEIDENTIFIER NOT NULL,
	"Balance" DECIMAL NOT NULL,
	"AccountName" NVARCHAR(50) NOT NULL
);
ALTER TABLE
	"trader_account" ADD CONSTRAINT "trader_account_traderid_primary" PRIMARY KEY("TraderID");
ALTER TABLE
	"trader_account" ADD CONSTRAINT "trader_account_userid_foreign" FOREIGN KEY("UserID") REFERENCES "user"("UserID");

-- Creating trade table
CREATE TABLE "trade"(
	"TradeID" UNIQUEIDENTIFIER DEFAULT NEWID() NOT NULL,
	"TraderID" UNIQUEIDENTIFIER NOT NULL,
	"CommodityID" UNIQUEIDENTIFIER NOT NULL,
	"PricePerUnit" DECIMAL NOT NULL,
	"Quantity" TINYINT NOT NULL,
	"IsBuy" BIT NOT NULL,
	"Expiry" DATETIME NOT NULL,
	"CreatedAt" DATETIME NOT NULL,
	"Bourse" NVARCHAR(10) NOT NULL,
	"MitigationID" UNIQUEIDENTIFIER NOT NULL,
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
	co."Country",
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
	"commodity" c ON t."CommodityID" = c."CommodityID"
LEFT JOIN
	"country" co ON u."CountryID" = co."CountryID";
GO


-- Seeding country table with UN recognized countries and observer states
INSERT INTO "country" ("Country")
VALUES
    ('Afghanistan'),
    ('Albania'),
    ('Algeria'),
    ('Andorra'),
    ('Angola'),
    ('Antigua and Barbuda'),
    ('Argentina'),
    ('Armenia'),
    ('Australia'),
    ('Austria'),
    ('Azerbaijan'),
    ('Bahamas'),
    ('Bahrain'),
    ('Bangladesh'),
    ('Barbados'),
    ('Belarus'),
    ('Belgium'),
    ('Belize'),
    ('Benin'),
    ('Bhutan'),
    ('Bolivia'),
    ('Bosnia and Herzegovina'),
    ('Botswana'),
    ('Brazil'),
    ('Brunei Darussalam'),
    ('Bulgaria'),
    ('Burkina Faso'),
    ('Burundi'),
    ('Cabo Verde'),
    ('Cambodia'),
    ('Cameroon'),
    ('Canada'),
    ('Central African Republic'),
    ('Chad'),
    ('Chile'),
    ('China'),
    ('Colombia'),
    ('Comoros'),
    ('Congo (Congo-Brazzaville)'),
    ('Congo (Democratic Republic)'),
    ('Costa Rica'),
    ('Croatia'),
    ('Cuba'),
    ('Cyprus'),
    ('Czech Republic'),
    ('Denmark'),
    ('Djibouti'),
    ('Dominica'),
    ('Dominican Republic'),
    ('Ecuador'),
    ('Egypt'),
    ('El Salvador'),
    ('Equatorial Guinea'),
    ('Eritrea'),
    ('Estonia'),
    ('Eswatini'),
    ('Ethiopia'),
    ('Fiji'),
    ('Finland'),
    ('France'),
    ('Gabon'),
    ('Gambia'),
    ('Georgia'),
    ('Germany'),
    ('Ghana'),
    ('Greece'),
    ('Grenada'),
    ('Guatemala'),
    ('Guinea'),
    ('Guinea-Bissau'),
    ('Guyana'),
    ('Haiti'),
    ('Honduras'),
    ('Hungary'),
    ('Iceland'),
    ('India'),
    ('Indonesia'),
    ('Iran'),
    ('Iraq'),
    ('Ireland'),
    ('Israel'),
    ('Italy'),
    ('Jamaica'),
    ('Japan'),
    ('Jordan'),
    ('Kazakhstan'),
    ('Kenya'),
    ('Kiribati'),
    ('Korea (North)'),
    ('Korea (South)'),
    ('Kuwait'),
    ('Kyrgyzstan'),
    ('Laos'),
    ('Latvia'),
    ('Lebanon'),
    ('Lesotho'),
    ('Liberia'),
    ('Libya'),
    ('Liechtenstein'),
    ('Lithuania'),
    ('Luxembourg'),
    ('Madagascar'),
    ('Malawi'),
    ('Malaysia'),
    ('Maldives'),
    ('Mali'),
    ('Malta'),
    ('Marshall Islands'),
    ('Mauritania'),
    ('Mauritius'),
    ('Mexico'),
    ('Micronesia (Federated States of)'),
    ('Moldova'),
    ('Monaco'),
    ('Mongolia'),
    ('Montenegro'),
    ('Morocco'),
    ('Mozambique'),
    ('Myanmar'),
    ('Namibia'),
    ('Nauru'),
    ('Nepal'),
    ('Netherlands'),
    ('New Zealand'),
    ('Nicaragua'),
    ('Niger'),
    ('Nigeria'),
    ('North Macedonia'),
    ('Norway'),
    ('Oman'),
    ('Pakistan'),
    ('Palau'),
	  ('Palestine'),
    ('Panama'),
    ('Papua New Guinea'),
    ('Paraguay'),
    ('Peru'),
    ('Philippines'),
    ('Poland'),
    ('Portugal'),
    ('Qatar'),
    ('Romania'),
    ('Russia'),
    ('Rwanda'),
    ('Saint Kitts and Nevis'),
    ('Saint Lucia'),
    ('Saint Vincent and the Grenadines'),
    ('Samoa'),
    ('San Marino'),
    ('Sao Tome and Principe'),
    ('Saudi Arabia'),
    ('Senegal'),
    ('Serbia'),
    ('Seychelles'),
    ('Sierra Leone'),
    ('Singapore'),
    ('Slovakia'),
    ('Slovenia'),
    ('Solomon Islands'),
    ('Somalia'),
    ('South Africa'),
    ('South Sudan'),
    ('Spain'),
    ('Sri Lanka'),
    ('Sudan'),
    ('Suriname'),
    ('Sweden'),
    ('Switzerland'),
    ('Syria'),
    ('Taiwan'),
    ('Tajikistan'),
    ('Tanzania'),
    ('Thailand'),
    ('Timor-Leste'),
    ('Togo'),
    ('Tonga'),
    ('Trinidad and Tobago'),
    ('Tunisia'),
    ('Turkey'),
    ('Turkmenistan'),
    ('Tuvalu'),
    ('Uganda'),
    ('Ukraine'),
    ('United Arab Emirates'),
    ('United Kingdom'),
    ('United States of America'),
    ('Uruguay'),
    ('Uzbekistan'),
    ('Vanuatu'),
    ('Vatican City'),
    ('Venezuela'),
    ('Vietnam'),
    ('Yemen'),
    ('Zambia'),
    ('Zimbabwe');
