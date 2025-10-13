using System;
using GearShare.Api.Models;
using GearShare.Api.Domain.Entities;   // <-- asigură-te că ai entitățile aici
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GearShare.Api.Data
{
    public class AppDbContext
        : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Week 2: domain sets
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Listing> Listings => Set<Listing>();
        public DbSet<ItemImage> ItemImages => Set<ItemImage>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // Item
            b.Entity<Item>(e =>
            {
                e.Property(i => i.Title).IsRequired().HasMaxLength(120);
                e.Property(i => i.Description).HasMaxLength(2000);
                // enum ca int în DB (explicit, deși EF ar face oricum int)
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
                // pentru PostgreSQL: numeric(12,2) pe prețuri
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
        }
    }
}
