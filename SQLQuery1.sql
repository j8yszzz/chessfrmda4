-- ==================================================
-- COMPLETE CHESS GAME DATABASE SETUP
-- Run this entire script in SQL Server Object Explorer
-- ==================================================

-- Create the database
CREATE DATABASE ChessGameDB;
GO

USE ChessGameDB;
GO

-- ==================================================
-- TABLE 1: Users (for login/registration)
-- ==================================================
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Email VARCHAR(100) NULL,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);
GO

-- ==================================================
-- TABLE 2: GameStats (stores win/loss records per user)
-- ==================================================
CREATE TABLE GameStats (
    StatID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    GamesPlayed INT NOT NULL DEFAULT 0,
    Wins INT NOT NULL DEFAULT 0,
    Losses INT NOT NULL DEFAULT 0,
    Draws INT NOT NULL DEFAULT 0,
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_GameStats_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- ==================================================
-- TABLE 3: GameHistory (detailed record of each game)
-- ==================================================
CREATE TABLE GameHistory (
    GameID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    GameDate DATETIME NOT NULL DEFAULT GETDATE(),
    PlayerColor VARCHAR(10) NOT NULL,
    AIDifficulty INT NOT NULL,
    Result VARCHAR(10) NOT NULL,
    EndReason VARCHAR(50) NULL,
    MoveCount INT NULL,
    CONSTRAINT FK_GameHistory_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- ==================================================
-- TABLE 4: PlayerSettings (saves user preferences)
-- ==================================================
CREATE TABLE PlayerSettings (
    SettingID INT PRIMARY KEY IDENTITY(1,1),
    UserID INT NOT NULL,
    PreferredColor VARCHAR(10) NOT NULL DEFAULT 'White',
    PreferredDifficulty INT NOT NULL DEFAULT 3,
    LastUpdated DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_PlayerSettings_Users FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

-- ==================================================
-- VIEW: User Statistics with Win Percentage
-- ==================================================
CREATE VIEW vw_UserStatistics AS
SELECT 
    u.UserID,
    u.Username,
    ISNULL(gs.GamesPlayed, 0) AS GamesPlayed,
    ISNULL(gs.Wins, 0) AS Wins,
    ISNULL(gs.Losses, 0) AS Losses,
    ISNULL(gs.Draws, 0) AS Draws,
    CASE 
        WHEN ISNULL(gs.GamesPlayed, 0) > 0 
        THEN CAST(ISNULL(gs.Wins, 0) AS FLOAT) / gs.GamesPlayed * 100
        ELSE 0
    END AS WinPercentage,
    u.LastLogin
FROM Users u
LEFT JOIN GameStats gs ON u.UserID = gs.UserID;
GO

-- ==================================================
-- STORED PROCEDURE: User Login
-- ==================================================
CREATE PROCEDURE sp_UserLogin
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(255)
AS
BEGIN
    DECLARE @UserID INT;
    
    SELECT @UserID = UserID 
    FROM Users 
    WHERE Username = @Username AND PasswordHash = @PasswordHash;
    
    IF @UserID IS NOT NULL
    BEGIN
        UPDATE Users SET LastLogin = GETDATE() WHERE UserID = @UserID;
        SELECT UserID, Username, Email FROM Users WHERE UserID = @UserID;
    END
    ELSE
    BEGIN
        SELECT NULL AS UserID;
    END
END
GO

-- ==================================================
-- STORED PROCEDURE: User Registration
-- ==================================================
CREATE PROCEDURE sp_UserRegister
    @Username VARCHAR(50),
    @PasswordHash VARCHAR(255),
    @Email VARCHAR(100)
AS
BEGIN
    DECLARE @UserID INT;
    
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        SELECT -1 AS UserID;
        RETURN;
    END
    
    INSERT INTO Users (Username, PasswordHash, Email)
    VALUES (@Username, @PasswordHash, @Email);
    
    SET @UserID = SCOPE_IDENTITY();
    
    INSERT INTO GameStats (UserID, GamesPlayed, Wins, Losses, Draws)
    VALUES (@UserID, 0, 0, 0, 0);
    
    INSERT INTO PlayerSettings (UserID, PreferredColor, PreferredDifficulty)
    VALUES (@UserID, 'White', 3);
    
    SELECT @UserID AS UserID;
END
GO

-- ==================================================
-- Insert test user (username: test, password: test123)
-- Password hash for "test123" using SHA256
-- ==================================================
INSERT INTO Users (Username, PasswordHash, Email)
VALUES ('test', 'ecd71870d1963316a97e3ac3408c9835ad8cf0f3c1bc703527c30265534f75ae', 'test@example.com');

DECLARE @TestUserID INT = SCOPE_IDENTITY();

INSERT INTO GameStats (UserID, GamesPlayed, Wins, Losses, Draws)
VALUES (@TestUserID, 0, 0, 0, 0);

INSERT INTO PlayerSettings (UserID, PreferredColor, PreferredDifficulty)
VALUES (@TestUserID, 'White', 3);
GO

PRINT 'Database setup complete!';
PRINT 'Test account created: Username = test, Password = test123';
GO