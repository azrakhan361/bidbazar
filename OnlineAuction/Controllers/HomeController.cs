using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Data;
using OnlineAuction.Models;

namespace OnlineAuction.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuctionDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(AuctionDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var categories = await _db.Categories
                .Include(c => c.Items.Where(i => i.BidStatus == 'A' && i.BidEndDate > DateTime.Now))
                .ToListAsync();

            var model = new HomeViewModel
            {
                Categories = categories,
                SearchQuery = search
            };

            if (!string.IsNullOrWhiteSpace(search))
            {
                model.SearchResults = await _db.Items
                    .Include(i => i.Category)
                    .Include(i => i.Seller)
                    .Where(i => i.BidStatus == 'A'
                             && i.BidEndDate > DateTime.Now
                             && (i.Title.ToLower().Contains(search.ToLower())
                                 || (i.Description != null && i.Description.ToLower().Contains(search.ToLower()))))
                    .ToListAsync();
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                    model.UnreadNotifications = await _db.Notifications
                        .CountAsync(n => n.UserId == user.Id && !n.IsRead);
            }

            // Auto-expire auctions
            var expiredItems = await _db.Items
                .Where(i => i.BidStatus == 'A' && i.BidEndDate <= DateTime.Now)
                .ToListAsync();

            foreach (var item in expiredItems)
            {
                item.BidStatus = 'I';
                // Notify seller
                _db.Notifications.Add(new Notification
                {
                    UserId = item.SellerId,
                    Message = $"Your auction for '{item.Title}' has ended. " +
                              (item.CurrentHighBidderId != null
                                  ? "A buyer has been found. Please contact them."
                                  : "No bids were placed.")
                });
                // Notify winner
                if (item.CurrentHighBidderId != null)
                {
                    _db.Notifications.Add(new Notification
                    {
                        UserId = item.CurrentHighBidderId,
                        Message = $"Congratulations! You won the auction for '{item.Title}'. Please contact the seller."
                    });
                }
            }

            if (expiredItems.Any()) await _db.SaveChangesAsync();

            return View(model);
        }
    }
}
