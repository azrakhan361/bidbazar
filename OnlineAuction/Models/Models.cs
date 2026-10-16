using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace OnlineAuction.Models
{
    // ─── Identity User ────────────────────────────────────────────────────────
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string FullName { get; set; } = "";
        public bool IsBlocked { get; set; } = false;
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public int CumulativeRating { get; set; } = 0;

        public virtual ICollection<Item> ItemsForSale { get; set; } = new List<Item>();
        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
        public virtual ICollection<Rating> RatingsGiven { get; set; } = new List<Rating>();
        public virtual ICollection<Rating> RatingsReceived { get; set; } = new List<Rating>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }

    // ─── Category ─────────────────────────────────────────────────────────────
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(300)]
        public string? Description { get; set; }

        public virtual ICollection<Item> Items { get; set; } = new List<Item>();
    }

    // ─── Item ─────────────────────────────────────────────────────────────────
    public class Item
    {
        [Key]
        public int ItemId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Item Title")]
        public string Title { get; set; } = "";

        [StringLength(4000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Image Path")]
        public string? ImagePath { get; set; }

        [Display(Name = "Document Path")]
        public string? DocumentPath { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Minimum Bid Price")]
        public decimal MinimumBid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Current Bid")]
        public decimal CurrentBid { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Bid Increment")]
        public decimal BidIncrement { get; set; }

        [Display(Name = "Bid Start Date")]
        public DateTime BidStartDate { get; set; } = DateTime.Now;

        [Display(Name = "Auction End Date")]
        public DateTime BidEndDate { get; set; }

        // 'A' = Active, 'I' = Inactive
        public char BidStatus { get; set; } = 'A';

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public string SellerId { get; set; } = "";
        [ForeignKey("SellerId")]
        public virtual ApplicationUser? Seller { get; set; }

        public string? CurrentHighBidderId { get; set; }

        public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();
    }

    // ─── Bid ──────────────────────────────────────────────────────────────────
    public class Bid
    {
        [Key]
        public int BidId { get; set; }

        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        public string BidderId { get; set; } = "";
        [ForeignKey("BidderId")]
        public virtual ApplicationUser? Bidder { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public DateTime BidTime { get; set; } = DateTime.Now;
    }

    // ─── Rating ───────────────────────────────────────────────────────────────
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }

        public string RaterId { get; set; } = "";
        [ForeignKey("RaterId")]
        public virtual ApplicationUser? Rater { get; set; }

        public string RatedUserId { get; set; } = "";
        [ForeignKey("RatedUserId")]
        public virtual ApplicationUser? RatedUser { get; set; }

        [Range(-5, 5)]
        public int Score { get; set; }

        [StringLength(500)]
        public string? Comment { get; set; }

        public DateTime RatedAt { get; set; } = DateTime.Now;
    }

    // ─── Notification ─────────────────────────────────────────────────────────
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string UserId { get; set; } = "";
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        [StringLength(500)]
        public string Message { get; set; } = "";

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    // ─── ViewModels ───────────────────────────────────────────────────────────
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public string? SearchQuery { get; set; }
        public List<Item> SearchResults { get; set; } = new();
        public int UnreadNotifications { get; set; }
    }

    public class ItemDetailViewModel
    {
        public Item Item { get; set; } = new();
        public List<Bid> BidHistory { get; set; } = new();
        public decimal NextMinimumBid { get; set; }
        public bool IsCurrentUserSeller { get; set; }
        public bool AuctionEnded { get; set; }
    }

    public class SellItemViewModel
    {
        [Required, StringLength(100)]
        public string Title { get; set; } = "";

        [StringLength(4000)]
        public string? Description { get; set; }

        public IFormFile? Image { get; set; }
        public IFormFile? Document { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Minimum bid must be positive")]
        [Display(Name = "Minimum Bid Price (PKR)")]
        public decimal MinimumBid { get; set; }

        [Required]
        [Display(Name = "Auction End Date")]
        public DateTime BidEndDate { get; set; } = DateTime.Now.AddDays(7);

        [Required, Range(0.01, double.MaxValue)]
        [Display(Name = "Bid Increment (PKR)")]
        public decimal BidIncrement { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<Category> Categories { get; set; } = new();
    }

    public class RegisterViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, StringLength(100), MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class LoginViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(Name = "Remember Me")]
        public bool RememberMe { get; set; }
    }

    public class MergeCategoryViewModel
    {
        [Required]
        [Display(Name = "Source Category (will be removed)")]
        public int SourceCategoryId { get; set; }

        [Required]
        [Display(Name = "Target Category (items moved here)")]
        public int TargetCategoryId { get; set; }

        public List<Category> Categories { get; set; } = new();
    }
}
