using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Data;
using OnlineAuction.Models;

namespace OnlineAuction.Controllers
{
    [Authorize]
    public class ItemController : Controller
    {
        private readonly AuctionDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public ItemController(AuctionDbContext db,
                               UserManager<ApplicationUser> userManager,
                               IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // ── Category Browse ──────────────────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> Category(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var items = await _db.Items
                .Include(i => i.Seller)
                .Where(i => i.CategoryId == id
                         && i.BidStatus == 'A'
                         && i.BidEndDate > DateTime.Now)
                .OrderByDescending(i => i.BidStartDate)
                .ToListAsync();

            ViewBag.CategoryName = category.Name;
            return View(items);
        }

        // ── Item Detail ──────────────────────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _db.Items
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .FirstOrDefaultAsync(i => i.ItemId == id);

            if (item == null) return NotFound();

            var bids = await _db.Bids
                .Include(b => b.Bidder)
                .Where(b => b.ItemId == id)
                .OrderByDescending(b => b.BidTime)
                .ToListAsync();

            var currentUser = await _userManager.GetUserAsync(User);

            var model = new ItemDetailViewModel
            {
                Item = item,
                BidHistory = bids,
                NextMinimumBid = item.CurrentBid > 0
                    ? item.CurrentBid + item.BidIncrement
                    : item.MinimumBid,
                IsCurrentUserSeller = currentUser?.Id == item.SellerId,
                AuctionEnded = item.BidEndDate <= DateTime.Now || item.BidStatus == 'I'
            };

            return View(model);
        }

        // ── Sell Item (GET) ──────────────────────────────────────────────────
        public async Task<IActionResult> Sell()
        {
            var model = new SellItemViewModel
            {
                Categories = await _db.Categories.ToListAsync()
            };
            return View(model);
        }

        // ── Sell Item (POST) ─────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Sell(SellItemViewModel model)
        {
            model.Categories = await _db.Categories.ToListAsync();
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var item = new Item
            {
                Title = model.Title,
                Description = model.Description,
                MinimumBid = model.MinimumBid,
                CurrentBid = 0,
                BidIncrement = model.BidIncrement,
                BidStartDate = DateTime.Now,
                BidEndDate = model.BidEndDate,
                BidStatus = 'A',
                CategoryId = model.CategoryId,
                SellerId = user.Id
            };

            // Save image
            if (model.Image != null && model.Image.Length > 0)
            {
                var imgFolder = Path.Combine(_env.WebRootPath, "uploads", "images");
                Directory.CreateDirectory(imgFolder);
                var imgFile = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(imgFolder, imgFile));
                await model.Image.CopyToAsync(stream);
                item.ImagePath = $"/uploads/images/{imgFile}";
            }

            // Save document
            if (model.Document != null && model.Document.Length > 0)
            {
                var docFolder = Path.Combine(_env.WebRootPath, "uploads", "docs");
                Directory.CreateDirectory(docFolder);
                var docFile = $"{Guid.NewGuid()}{Path.GetExtension(model.Document.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(docFolder, docFile));
                await model.Document.CopyToAsync(stream);
                item.DocumentPath = $"/uploads/docs/{docFile}";
            }

            _db.Items.Add(item);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Item listed successfully!";
            return RedirectToAction("Details", new { id = item.ItemId });
        }

        // ── Edit Item (GET) ──────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.Id != item.SellerId) return Forbid();

            ViewBag.Item = item;
            return View(item);
        }

        // ── Edit Item (POST) ─────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? newImage, IFormFile? newDocument, decimal bidIncrement)
        {
            var item = await _db.Items.FindAsync(id);
            if (item == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user?.Id != item.SellerId) return Forbid();

            item.BidIncrement = bidIncrement;

            if (newImage != null && newImage.Length > 0)
            {
                var imgFolder = Path.Combine(_env.WebRootPath, "uploads", "images");
                Directory.CreateDirectory(imgFolder);
                var imgFile = $"{Guid.NewGuid()}{Path.GetExtension(newImage.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(imgFolder, imgFile));
                await newImage.CopyToAsync(stream);
                item.ImagePath = $"/uploads/images/{imgFile}";
            }

            if (newDocument != null && newDocument.Length > 0)
            {
                var docFolder = Path.Combine(_env.WebRootPath, "uploads", "docs");
                Directory.CreateDirectory(docFolder);
                var docFile = $"{Guid.NewGuid()}{Path.GetExtension(newDocument.FileName)}";
                using var stream = System.IO.File.Create(Path.Combine(docFolder, docFile));
                await newDocument.CopyToAsync(stream);
                item.DocumentPath = $"/uploads/docs/{docFile}";
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Item updated successfully!";
            return RedirectToAction("Details", new { id });
        }

        // ── Place Bid ────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(int itemId, decimal bidAmount)
        {
            var item = await _db.Items.FindAsync(itemId);
            if (item == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (user.Id == item.SellerId)
            {
                TempData["Error"] = "You cannot bid on your own item.";
                return RedirectToAction("Details", new { id = itemId });
            }

            if (item.BidEndDate <= DateTime.Now || item.BidStatus == 'I')
            {
                TempData["Error"] = "This auction has ended.";
                return RedirectToAction("Details", new { id = itemId });
            }

            decimal minRequired = item.CurrentBid > 0
                ? item.CurrentBid + item.BidIncrement
                : item.MinimumBid;

            if (bidAmount < minRequired)
            {
                TempData["Error"] = $"Bid must be at least PKR {minRequired:N0}.";
                return RedirectToAction("Details", new { id = itemId });
            }

            item.CurrentBid = bidAmount;
            item.CurrentHighBidderId = user.Id;

            _db.Bids.Add(new Bid
            {
                ItemId = itemId,
                BidderId = user.Id,
                Amount = bidAmount,
                BidTime = DateTime.Now
            });

            // Notify seller of new bid
            _db.Notifications.Add(new Notification
            {
                UserId = item.SellerId,
                Message = $"New bid of PKR {bidAmount:N0} placed on '{item.Title}'."
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Bid placed successfully!";
            return RedirectToAction("Details", new { id = itemId });
        }

        // ── Rate User ────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Rate(int itemId, string ratedUserId, int score, string? comment)
        {
            var item = await _db.Items.FindAsync(itemId);
            if (item == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Unauthorized();

            // Check if already rated
            var existing = await _db.Ratings
                .FirstOrDefaultAsync(r => r.ItemId == itemId && r.RaterId == currentUser.Id);
            if (existing != null)
            {
                TempData["Error"] = "You have already rated this user for this item.";
                return RedirectToAction("Details", new { id = itemId });
            }

            _db.Ratings.Add(new Rating
            {
                ItemId = itemId,
                RaterId = currentUser.Id,
                RatedUserId = ratedUserId,
                Score = score,
                Comment = comment
            });

            // Update cumulative rating
            var ratedUser = await _userManager.FindByIdAsync(ratedUserId);
            if (ratedUser != null)
            {
                ratedUser.CumulativeRating += score;
                await _userManager.UpdateAsync(ratedUser);
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Rating submitted!";
            return RedirectToAction("Details", new { id = itemId });
        }

        // ── Notifications ────────────────────────────────────────────────────
        public async Task<IActionResult> Notifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notifications = await _db.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Mark all as read
            foreach (var n in notifications.Where(n => !n.IsRead))
                n.IsRead = true;
            await _db.SaveChangesAsync();

            return View(notifications);
        }

        // ── Seller Profile ───────────────────────────────────────────────────
        [AllowAnonymous]
        public async Task<IActionResult> SellerProfile(string id)
        {
            var seller = await _userManager.FindByIdAsync(id);
            if (seller == null) return NotFound();

            var ratings = await _db.Ratings
                .Include(r => r.Rater)
                .Where(r => r.RatedUserId == id)
                .ToListAsync();

            ViewBag.Seller = seller;
            ViewBag.Ratings = ratings;
            return View();
        }
    }
}
