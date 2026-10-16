using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Data;
using OnlineAuction.Models;

namespace OnlineAuction.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AuctionDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AuctionDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ── Dashboard ────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _userManager.Users.CountAsync();
            ViewBag.BlockedUsers = await _userManager.Users.CountAsync(u => u.IsBlocked);
            ViewBag.TotalItems = await _db.Items.CountAsync();
            ViewBag.ActiveItems = await _db.Items.CountAsync(i => i.BidStatus == 'A');
            ViewBag.TotalCategories = await _db.Categories.CountAsync();
            ViewBag.TotalBids = await _db.Bids.CountAsync();
            return View();
        }

        // ── Manage Users ─────────────────────────────────────────────────────
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        // ── Block/Unblock User ───────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBlock(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsBlocked = !user.IsBlocked;
            await _userManager.UpdateAsync(user);

            TempData["Success"] = user.IsBlocked
                ? $"{user.FullName} has been blocked."
                : $"{user.FullName} has been unblocked.";

            return RedirectToAction("Users");
        }

        // ── Categories List ───────────────────────────────────────────────────
        public async Task<IActionResult> Categories()
        {
            var cats = await _db.Categories
                .Include(c => c.Items)
                .ToListAsync();
            return View(cats);
        }

        // ── Create Category ───────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string name, string? description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                TempData["Error"] = "Category name is required.";
                return RedirectToAction("Categories");
            }

            _db.Categories.Add(new Category { Name = name, Description = description });
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Category '{name}' created.";
            return RedirectToAction("Categories");
        }

        // ── Merge Category (GET) ──────────────────────────────────────────────
        public async Task<IActionResult> MergeCategory()
        {
            var model = new MergeCategoryViewModel
            {
                Categories = await _db.Categories.ToListAsync()
            };
            return View(model);
        }

        // ── Merge Category (POST) ─────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MergeCategory(MergeCategoryViewModel model)
        {
            if (model.SourceCategoryId == model.TargetCategoryId)
            {
                TempData["Error"] = "Source and target must be different.";
                model.Categories = await _db.Categories.ToListAsync();
                return View(model);
            }

            var sourceItems = await _db.Items
                .Where(i => i.CategoryId == model.SourceCategoryId)
                .ToListAsync();

            foreach (var item in sourceItems)
                item.CategoryId = model.TargetCategoryId;

            var source = await _db.Categories.FindAsync(model.SourceCategoryId);
            if (source != null) _db.Categories.Remove(source);

            await _db.SaveChangesAsync();
            TempData["Success"] = "Categories merged successfully.";
            return RedirectToAction("Categories");
        }

        // ── All Items ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Items()
        {
            var items = await _db.Items
                .Include(i => i.Category)
                .Include(i => i.Seller)
                .OrderByDescending(i => i.BidStartDate)
                .ToListAsync();
            return View(items);
        }
    }
}
