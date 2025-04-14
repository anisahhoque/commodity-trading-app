-- Seed commodities
INSERT INTO dbo.commodity(CommodityName)
VALUES ('gold');

-- Create SuperUser with all roles
-- Insert a new SuperUser into the user table
DECLARE @NewUserId UNIQUEIDENTIFIER = NEWID();

INSERT INTO [user] (UserID, Username, PasswordHash, CountryID)
VALUES (
    @NewUserId,
    'SuperUser',
    '$2a$12$lBF6dFslu7nV9W7yep1xvONcuDyg5nib5jeCb28Yh2c/q71/JqxTC',  -- Password is "SuperUserPassword7!"
    1  -- CountryID doesn't matter
);

-- Assign all roles to SuperUser
INSERT INTO role_assignment (AssignmentID, UserID, RoleID)
SELECT 
    NEWID(),
    @NewUserId,
    RoleID
FROM 
    role;
