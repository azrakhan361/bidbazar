# BidBazaar — Online Auction System (.NET 8 MVC)

## Project Structure
```
OnlineAuction/
├── Controllers/
│   ├── HomeController.cs       ← Dashboard + Search
│   ├── AccountController.cs    ← Register / Login / Logout
│   ├── ItemController.cs       ← Sell, Bid, Edit, Rate, Notifications
│   └── AdminController.cs      ← Block Users, Categories, Merge
├── Models/
│   └── Models.cs               ← All models + ViewModels
├── Data/
│   └── AuctionDbContext.cs     ← EF Core DbContext
├── Views/
│   ├── Home/Index.cshtml       ← Dashboard
│   ├── Account/Login.cshtml
│   ├── Account/Register.cshtml
│   ├── Item/Details.cshtml     ← Item page + Bidding
│   ├── Item/Category.cshtml    ← Category browse
│   ├── Item/Sell.cshtml
│   ├── Item/Edit.cshtml
│   ├── Item/Notifications.cshtml
│   ├── Item/SellerProfile.cshtml
│   ├── Admin/Index.cshtml      ← Admin dashboard
│   ├── Admin/Users.cshtml
│   ├── Admin/Categories.cshtml
│   ├── Admin/MergeCategory.cshtml
│   └── Admin/Items.cshtml
├── Program.cs
├── appsettings.json
└── DatabaseSetup.sql           ← Manual SQL (optional)
```

## Setup Instructions

### Prerequisites
- Visual Studio 2022 (or VS Code)
- .NET 8 SDK
- SQL Server (LocalDB or Express)

### Step 1 — Open the Project
Open `OnlineAuction.csproj` in Visual Studio.

### Step 2 — Install NuGet Packages
In Package Manager Console:
```
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.AspNetCore.Identity.EntityFrameworkCore
Install-Package Microsoft.AspNetCore.Identity.UI
```

Or use `dotnet restore` from command line.

### Step 3 — Configure Database
In `appsettings.json`, update connection string if needed:
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OnlineAuctionDB;Trusted_Connection=True;"
```

### Step 4 — Run Migrations
In Package Manager Console:
```
Add-Migration InitialCreate
Update-Database
```

This creates all tables and seeds 5 default categories.

### Step 5 — Run the Project
Press **F5** in Visual Studio.

### Default Admin Account
- **Email:** admin@auction.com  
- **Password:** Admin@123

---

## Features Implemented

| Feature | Status |
|---------|--------|
| User Registration & Login | ✅ |
| Role-based Access (Admin/User) | ✅ |
| Block Fraudulent Users | ✅ |
| Search (case-insensitive) | ✅ |
| Browse by Category | ✅ |
| Sell Item (with image + doc upload) | ✅ |
| Edit Item (image, doc, increment) | ✅ |
| Place Bid (with validation) | ✅ |
| Bid History | ✅ |
| Auction Auto-expiry | ✅ |
| Seller/Buyer Notifications | ✅ |
| Rating System (-5 to +5) | ✅ |
| Seller Profile Page | ✅ |
| Admin: Add Category | ✅ |
| Admin: Merge Categories | ✅ |
| Admin: View All Items | ✅ |
| Responsive Bootstrap UI | ✅ |
