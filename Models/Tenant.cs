using SaaSEventos.Models.Enums;

namespace SaaSEventos.Models;

public class Tenant
{
    public int Id { get; set; }
    public int? AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public Account? Account { get; set; }
    public List<User> Users { get; set; } = new();
    public List<Product> Products { get; set; } = new();
    public List<Event> Events { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
    public List<Coupon> Coupons { get; set; } = new();
}
