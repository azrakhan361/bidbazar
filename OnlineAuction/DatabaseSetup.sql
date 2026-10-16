-- ============================================================
-- BidBazaar - Online Auction System
-- SQL Setup Script (if NOT using EF Core Migrations)
-- Run this in SQL Server Management Studio
-- ============================================================

CREATE DATABASE OnlineAuctionDB;
GO
USE OnlineAuctionDB;
GO

-- Note: With EF Core + Identity, run these commands instead:
-- dotnet ef migrations add InitialCreate
-- dotnet ef database update
-- The above will auto-create all tables.

-- ── Manual table creation (for reference only) ────────────────────────────

-- Categories
CREATE TABLE Categories (
    CategoryId   INT PRIMARY KEY IDENTITY,
    Name         NVARCHAR(100) NOT NULL,
    Description  NVARCHAR(300) NULL
);

-- Insert default categories
INSERT INTO Categories (Name, Description) VALUES
('Electronics',  'Phones, laptops, gadgets'),
('Furniture',    'Home and office furniture'),
('Books',        'All kinds of books'),
('Clothing',     'Clothes and accessories'),
('Vehicles',     'Cars, bikes, and more');

-- Items
CREATE TABLE Items (
    ItemId          INT PRIMARY KEY IDENTITY,
    Title           NVARCHAR(100) NOT NULL,
    Description     NVARCHAR(4000) NULL,
    ImagePath       NVARCHAR(500) NULL,
    DocumentPath    NVARCHAR(500) NULL,
    MinimumBid      DECIMAL(18,2) NOT NULL DEFAULT 0,
    CurrentBid      DECIMAL(18,2) NOT NULL DEFAULT 0,
    BidIncrement    DECIMAL(18,2) NOT NULL DEFAULT 100,
    BidStartDate    DATETIME NOT NULL DEFAULT GETDATE(),
    BidEndDate      DATETIME NOT NULL,
    BidStatus       CHAR(1) NOT NULL DEFAULT 'A',  -- A=Active, I=Inactive
    CategoryId      INT NOT NULL REFERENCES Categories(CategoryId),
    SellerId        NVARCHAR(450) NOT NULL,         -- FK to AspNetUsers
    CurrentHighBidderId NVARCHAR(450) NULL
);

-- Bids
CREATE TABLE Bids (
    BidId      INT PRIMARY KEY IDENTITY,
    ItemId     INT NOT NULL REFERENCES Items(ItemId) ON DELETE CASCADE,
    BidderId   NVARCHAR(450) NOT NULL,
    Amount     DECIMAL(18,2) NOT NULL,
    BidTime    DATETIME NOT NULL DEFAULT GETDATE()
);

-- Ratings
CREATE TABLE Ratings (
    RatingId     INT PRIMARY KEY IDENTITY,
    ItemId       INT NOT NULL REFERENCES Items(ItemId),
    RaterId      NVARCHAR(450) NOT NULL,
    RatedUserId  NVARCHAR(450) NOT NULL,
    Score        INT NOT NULL CHECK (Score BETWEEN -5 AND 5),
    Comment      NVARCHAR(500) NULL,
    RatedAt      DATETIME NOT NULL DEFAULT GETDATE()
);

-- Notifications
CREATE TABLE Notifications (
    NotificationId INT PRIMARY KEY IDENTITY,
    UserId         NVARCHAR(450) NOT NULL,
    Message        NVARCHAR(500) NOT NULL,
    IsRead         BIT NOT NULL DEFAULT 0,
    CreatedAt      DATETIME NOT NULL DEFAULT GETDATE()
);

PRINT 'Database setup complete!';
