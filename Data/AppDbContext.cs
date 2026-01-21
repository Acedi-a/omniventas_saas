using Microsoft.EntityFrameworkCore;
using SaaSEventos.Models;

namespace SaaSEventos.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Coupon> Coupons => Set<Coupon>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>().HasIndex(t => t.ApiKey).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => new { u.TenantId, u.Email }).IsUnique();
        modelBuilder.Entity<Product>().HasIndex(p => p.TenantId);
        modelBuilder.Entity<Event>().HasIndex(e => e.TenantId);
        modelBuilder.Entity<Order>().HasIndex(o => new { o.TenantId, o.UserId });
        modelBuilder.Entity<Ticket>().HasIndex(t => t.Code).IsUnique();
        modelBuilder.Entity<Coupon>().HasIndex(c => new { c.TenantId, c.Code }).IsUnique();

        modelBuilder.Entity<Product>().Property(p => p.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Event>().Property(e => e.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Order>().Property(o => o.Subtotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Order>().Property(o => o.Discount).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Order>().Property(o => o.Total).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<OrderItem>().Property(oi => oi.Subtotal).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Coupon>().Property(c => c.DiscountPercentage).HasColumnType("decimal(5,2)");
    }
}
