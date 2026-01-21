using Microsoft.EntityFrameworkCore;
using SaaSEventos.Models;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Data;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var hasActiveFlag = await db.Tenants.AnyAsync(t => t.IsActive);
        if (!hasActiveFlag && await db.Tenants.AnyAsync())
        {
            var tenantsToActivate = await db.Tenants.ToListAsync();
            foreach (var tenant in tenantsToActivate)
            {
                tenant.IsActive = true;
            }

            await db.SaveChangesAsync();
        }
        var platformTenant = await db.Tenants.FirstOrDefaultAsync(t => t.Name == "SaaS Platform");

        if (platformTenant == null)
        {
            platformTenant = new Tenant
            {
                Name = "SaaS Platform",
                ApiKey = $"tn_live_{Guid.NewGuid():N}",
                BusinessType = BusinessType.Hybrid,
                IsActive = true,
                CreatedAt = now
            };

            db.Tenants.Add(platformTenant);
            await db.SaveChangesAsync();
        }

        var superAdminExists = await db.Users.AnyAsync(u => u.Role == UserRole.SuperAdmin);
        if (!superAdminExists)
        {
            var superAdmin = new User
            {
                TenantId = platformTenant.Id,
                Email = "superadmin@saaseventos.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.SuperAdmin,
                CreatedAt = now
            };

            db.Users.Add(superAdmin);
            await db.SaveChangesAsync();
        }

        var hasBusinessTenants = await db.Tenants.AnyAsync(t => t.Name != "SaaS Platform");
        if (hasBusinessTenants)
        {
            return;
        }

        var tenants = new List<Tenant>
        {
            new Tenant
            {
                Name = "Vinos Aranjuez",
                ApiKey = $"tn_live_{Guid.NewGuid():N}",
                BusinessType = BusinessType.Commerce,
                IsActive = true,
                CreatedAt = now
            },
            new Tenant
            {
                Name = "Teatro Municipal",
                ApiKey = $"tn_live_{Guid.NewGuid():N}",
                BusinessType = BusinessType.Events,
                IsActive = true,
                CreatedAt = now
            }
        };

        db.Tenants.AddRange(tenants);
        await db.SaveChangesAsync();

        var admins = new List<User>
        {
            new User
            {
                TenantId = tenants[0].Id,
                Email = "admin@vinosaranjuez.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = now
            },
            new User
            {
                TenantId = tenants[1].Id,
                Email = "admin@teatromunicipal.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                CreatedAt = now
            }
        };

        db.Users.AddRange(admins);
        await db.SaveChangesAsync();

        var products = new List<Product>
        {
            new Product { TenantId = tenants[0].Id, Name = "Cabernet 2020", Description = "Red wine bottle", Price = 120m, Stock = 50, CreatedAt = now },
            new Product { TenantId = tenants[0].Id, Name = "Syrah Reserva", Description = "Reserve blend", Price = 150m, Stock = 40, CreatedAt = now },
            new Product { TenantId = tenants[0].Id, Name = "Merlot Clasico", Description = "Merlot classic", Price = 90m, Stock = 60, CreatedAt = now },
            new Product { TenantId = tenants[0].Id, Name = "Blend Premium", Description = "Premium blend", Price = 200m, Stock = 30, CreatedAt = now },
            new Product { TenantId = tenants[0].Id, Name = "Rose Verano", Description = "Summer rose", Price = 80m, Stock = 70, CreatedAt = now },
            new Product { TenantId = tenants[1].Id, Name = "Poster Hamlet", Description = "Limited poster", Price = 40m, Stock = 100, CreatedAt = now },
            new Product { TenantId = tenants[1].Id, Name = "Program Book", Description = "Event booklet", Price = 25m, Stock = 150, CreatedAt = now },
            new Product { TenantId = tenants[1].Id, Name = "VIP Badge", Description = "VIP badge", Price = 60m, Stock = 80, CreatedAt = now },
            new Product { TenantId = tenants[1].Id, Name = "Theatre Mug", Description = "Souvenir mug", Price = 35m, Stock = 90, CreatedAt = now },
            new Product { TenantId = tenants[1].Id, Name = "Gift Card", Description = "Gift card", Price = 100m, Stock = 200, CreatedAt = now }
        };

        var events = new List<Event>
        {
            new Event { TenantId = tenants[0].Id, Name = "Wine Tasting", EventDate = now.AddDays(20), Location = "Aranjuez Cellar", MaxCapacity = 50, AvailableTickets = 50, Price = 180m, CreatedAt = now },
            new Event { TenantId = tenants[0].Id, Name = "Harvest Tour", EventDate = now.AddDays(45), Location = "Vineyard", MaxCapacity = 40, AvailableTickets = 40, Price = 220m, CreatedAt = now },
            new Event { TenantId = tenants[0].Id, Name = "Sommelier Night", EventDate = now.AddDays(60), Location = "Tasting Hall", MaxCapacity = 30, AvailableTickets = 30, Price = 260m, CreatedAt = now },
            new Event { TenantId = tenants[1].Id, Name = "Hamlet", EventDate = now.AddDays(15), Location = "Main Stage", MaxCapacity = 300, AvailableTickets = 300, Price = 70m, CreatedAt = now },
            new Event { TenantId = tenants[1].Id, Name = "Comedy Night", EventDate = now.AddDays(35), Location = "Blue Hall", MaxCapacity = 250, AvailableTickets = 250, Price = 55m, CreatedAt = now },
            new Event { TenantId = tenants[1].Id, Name = "Classics Gala", EventDate = now.AddDays(90), Location = "Grand Theatre", MaxCapacity = 400, AvailableTickets = 400, Price = 90m, CreatedAt = now }
        };

        var coupons = new List<Coupon>
        {
            new Coupon
            {
                TenantId = tenants[0].Id,
                Code = "TARIJA10",
                DiscountPercentage = 10m,
                MaxUses = 100,
                CurrentUses = 0,
                ExpiresAt = now.AddMonths(3)
            },
            new Coupon
            {
                TenantId = tenants[1].Id,
                Code = "TEATRO15",
                DiscountPercentage = 15m,
                MaxUses = 200,
                CurrentUses = 0,
                ExpiresAt = now.AddMonths(3)
            }
        };

        db.Products.AddRange(products);
        db.Events.AddRange(events);
        db.Coupons.AddRange(coupons);
        await db.SaveChangesAsync();
    }
}
