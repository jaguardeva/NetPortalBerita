using Microsoft.AspNetCore.Identity;
using PortalBeritaApp.Models;

namespace PortalBeritaApp.Data
{
    /// <summary>
    /// Seeds initial data: roles, default categories, and an admin user.
    /// Per PRD §8 (roles), §25 (default categories).
    /// </summary>
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // ── Seed Roles ──
            string[] roles = { "Admin", "Author", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }

            // ── Seed Admin User ──
            const string adminEmail = "admin@newsportal.com";
            const string adminPassword = "Admin@123456";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = "Administrator",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // ── Seed Default Categories (PRD §25) ──
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
                    new Category { Id = Guid.NewGuid(), Name = "Politik", Slug = "politik", CreatedAt = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Teknologi", Slug = "teknologi", CreatedAt = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Bisnis", Slug = "bisnis", CreatedAt = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Olahraga", Slug = "olahraga", CreatedAt = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Hiburan", Slug = "hiburan", CreatedAt = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Lifestyle", Slug = "lifestyle", CreatedAt = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Nasional", Slug = "nasional", CreatedAt = DateTime.UtcNow },
                    new Category { Id = Guid.NewGuid(), Name = "Internasional", Slug = "internasional", CreatedAt = DateTime.UtcNow },
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
        }
    }
}
