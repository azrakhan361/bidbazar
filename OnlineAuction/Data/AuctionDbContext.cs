using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineAuction.Models;

namespace OnlineAuction.Data
{
    public class AuctionDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuctionDbContext(DbContextOptions<AuctionDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Bid> Bids { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed default categories
            builder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Electronics", Description = "Phones, laptops, gadgets" },
                new Category { CategoryId = 2, Name = "Furniture", Description = "Home and office furniture" },
                new Category { CategoryId = 3, Name = "Books", Description = "All kinds of books" },
                new Category { CategoryId = 4, Name = "Clothing", Description = "Clothes and accessories" },
                new Category { CategoryId = 5, Name = "Vehicles", Description = "Cars, bikes, and more" }
            );

            // Prevent cascade delete conflicts
            builder.Entity<Rating>()
                .HasOne(r => r.Rater)
                .WithMany(u => u.RatingsGiven)
                .HasForeignKey(r => r.RaterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Rating>()
                .HasOne(r => r.RatedUser)
                .WithMany(u => u.RatingsReceived)
                .HasForeignKey(r => r.RatedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Bid>()
                .HasOne(b => b.Item)
                .WithMany(i => i.Bids)
                .HasForeignKey(b => b.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
