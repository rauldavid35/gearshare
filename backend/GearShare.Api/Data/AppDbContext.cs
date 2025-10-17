using GearShare.Api.Models;
using GearShare.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GearShare.Api.Data
{
    public class AppDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Item> Items => Set<Item>();
        public DbSet<Listing> Listings => Set<Listing>();
        public DbSet<ItemImage> ItemImages => Set<ItemImage>();
        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Item
            b.Entity<Item>(e =>
            {
                e.Property(i => i.Title).IsRequired().HasMaxLength(120);
                e.Property(i => i.Description).HasMaxLength(2000);
                e.Property(i => i.Category).HasConversion<int>();
                e.HasMany(i => i.Images)
                    .WithOne(img => img.Item)
                    .HasForeignKey(img => img.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
                e.HasMany(i => i.Listings)
                    .WithOne(l => l.Item)
                    .HasForeignKey(l => l.ItemId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Listing
            b.Entity<Listing>(e =>
            {
                e.Property(l => l.PricePerDay).HasColumnType("numeric(12,2)");
                e.Property(l => l.Deposit).HasColumnType("numeric(12,2)");
                e.HasIndex(l => l.ItemId);
            });

            // ItemImage
            b.Entity<ItemImage>(e =>
            {
                e.Property(ii => ii.FileName).IsRequired();
                e.Property(ii => ii.RelativePath).IsRequired();
                e.HasIndex(ii => new { ii.ItemId, ii.SortOrder });
            });

            // Booking (minimal config; table already exists in your DB)
            b.Entity<Booking>(e =>
            {
                e.Property(x => x.TotalPrice).HasColumnType("numeric(12,2)");
                e.Property(x => x.Status).HasConversion<int>();
                e.HasIndex(x => x.ListingId);
                e.HasOne(x => x.Listing)
                    .WithMany(l => l.Bookings)
                    .HasForeignKey(x => x.ListingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
