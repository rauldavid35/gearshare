using System;
using System.Linq;
using System.Threading.Tasks;
using GearShare.Api.Data;
using GearShare.Api.Domain.Entities;
using GearShare.Api.Domain.Enums;
using GearShare.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GearShare.Api.Data
{
    public static class SeedData
    {
        /// <summary>
        /// Rulează migrarea DB, creează roluri + utilizatori demo și populează câteva Items/Listings.
        /// </summary>
        public static async Task EnsureAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var db = sp.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();

            // 1) Roluri de bază
            var roleMgr = sp.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            foreach (var role in new[] { "OWNER", "RENTER", "ADMIN" })
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    await roleMgr.CreateAsync(new IdentityRole<Guid>(role));
            }

            // 2) Utilizatori demo
            var userMgr = sp.GetRequiredService<UserManager<ApplicationUser>>();
            var admin  = await EnsureUserAsync(userMgr, "admin@gearshare.local",  "Admin123!",  "Admin",  new[] { "ADMIN" });
            var owner  = await EnsureUserAsync(userMgr, "owner@gearshare.local",  "Owner123!",  "Owner",  new[] { "OWNER" });
            var renter = await EnsureUserAsync(userMgr, "renter@gearshare.local", "Renter123!", "Renter", new[] { "RENTER" });

            // 3) Date demo doar dacă nu există deja
            if (!await db.Items.AnyAsync())
            {
                var items = new[]
                {
                    new Item { Title = "Bicicletă MTB Hardtail", Category = ItemCategory.Sports, Condition = "GOOD", OwnerId = owner.Id, Description = "Cadru M, frâne disc, 29\"" },
                    new Item { Title = "Aparat foto Mirrorless",   Category = ItemCategory.Photo,  Condition = "LIKE_NEW", OwnerId = owner.Id, Description = "4K, 24MP, 2 baterii" },
                    new Item { Title = "Set bormașină + burghie",  Category = ItemCategory.DIY,    Condition = "GOOD", OwnerId = owner.Id, Description = "Valiză + accesorii" },
                    new Item { Title = "Cort 3 persoane",          Category = ItemCategory.Sports, Condition = "GOOD", OwnerId = owner.Id, Description = "Impermeabil, ușor" },
                    new Item { Title = "GoPro Hero",               Category = ItemCategory.Photo,  Condition = "GOOD", OwnerId = owner.Id, Description = "Suport casco inclus" },
                    new Item { Title = "Polizor unghiular",        Category = ItemCategory.DIY,    Condition = "FAIR", OwnerId = owner.Id, Description = "Discuri incluse" }
                };
                db.Items.AddRange(items);
                await db.SaveChangesAsync();

                // Listings pentru primele 4 obiecte
                var listings = new[]
                {
                    new Listing { ItemId = items[0].Id, PricePerDay = 40, Deposit = 150, LocationCity = "Bucharest", Active = true },
                    new Listing { ItemId = items[1].Id, PricePerDay = 90, Deposit = 400, LocationCity = "Bucharest", Active = true },
                    new Listing { ItemId = items[2].Id, PricePerDay = 30, Deposit = 100, LocationCity = "Bucharest", Active = true },
                    new Listing { ItemId = items[3].Id, PricePerDay = 35, Deposit = 120, LocationCity = "Bucharest", Active = true },
                };
                db.Listings.AddRange(listings);
                await db.SaveChangesAsync();
            }
        }

        private static async Task<ApplicationUser> EnsureUserAsync(
            UserManager<ApplicationUser> userMgr,
            string email, string password, string displayName, string[] roles)
        {
            var user = await userMgr.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    Id = Guid.NewGuid(),
                    UserName = email,
                    Email = email,
                    DisplayName = displayName,
                    EmailConfirmed = true
                };
                var res = await userMgr.CreateAsync(user, password);
                if (!res.Succeeded)
                    throw new InvalidOperationException("Cannot create demo user: " +
                        string.Join("; ", res.Errors.Select(e => e.Description)));
            }

            foreach (var role in roles)
            {
                if (!await userMgr.IsInRoleAsync(user, role))
                    await userMgr.AddToRoleAsync(user, role);
            }

            return user;
        }
    }
}
