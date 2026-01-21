using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Orders;
using SaaSEventos.Helpers;
using SaaSEventos.Models;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class OrderService
{
    private const int PaymentExpirationSeconds = 900;
    private readonly AppDbContext _db;
    private readonly CouponService _couponService;

    public OrderService(AppDbContext db, CouponService couponService)
    {
        _db = db;
        _couponService = couponService;
    }

    public async Task<OrderResponse> CreateOrderAsync(int tenantId, int userId, CreateOrderRequest request)
    {
        if (request.Items == null || request.Items.Count == 0)
        {
            throw new InvalidOperationException("Order must include at least one item.");
        }

        var order = new Order
        {
            TenantId = tenantId,
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            PaymentQRUrl = string.Empty
        };

        var orderItems = new List<OrderItem>();
        decimal subtotal = 0m;

        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException("Item quantity must be greater than zero.");
            }

            var hasProduct = item.ProductId.HasValue;
            var hasEvent = item.EventId.HasValue;
            if (hasProduct == hasEvent)
            {
                throw new InvalidOperationException("Each item must specify either productId or eventId.");
            }

            if (hasProduct)
            {
                var product = await _db.Products
                    .FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == item.ProductId);

                if (product == null)
                {
                    throw new InvalidOperationException("Product not found.");
                }

                if (product.Stock < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient stock for product.");
                }

                var lineSubtotal = product.Price * item.Quantity;
                subtotal += lineSubtotal;

                orderItems.Add(new OrderItem
                {
                    Order = order,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Subtotal = lineSubtotal
                });
            }
            else
            {
                var eventEntity = await _db.Events
                    .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == item.EventId);

                if (eventEntity == null)
                {
                    throw new InvalidOperationException("Event not found.");
                }

                if (eventEntity.AvailableTickets < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient tickets for event.");
                }

                var lineSubtotal = eventEntity.Price * item.Quantity;
                subtotal += lineSubtotal;

                orderItems.Add(new OrderItem
                {
                    Order = order,
                    EventId = eventEntity.Id,
                    Quantity = item.Quantity,
                    UnitPrice = eventEntity.Price,
                    Subtotal = lineSubtotal
                });
            }
        }

        order.Subtotal = subtotal;
        order.Discount = 0m;
        order.Total = subtotal;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var discount = await _couponService.ValidateAndApplyAsync(tenantId, request.CouponCode, subtotal);
            order.Discount = discount;
            order.Total = subtotal - discount;
        }
        order.Items = orderItems;

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        order.PaymentQRUrl = $"https://qr.com/order-{order.Id}";
        await _db.SaveChangesAsync();

        return new OrderResponse
        {
            OrderId = order.Id,
            Total = order.Total,
            PaymentQRUrl = order.PaymentQRUrl,
            ExpiresIn = PaymentExpirationSeconds
        };
    }

    public async Task<OrderDetailResponse?> GetOrderAsync(int tenantId, int userId, int orderId)
    {
        return await _db.Orders
            .Where(o => o.Id == orderId && o.TenantId == tenantId && o.UserId == userId)
            .Select(o => new OrderDetailResponse
            {
                Id = o.Id,
                Subtotal = o.Subtotal,
                Discount = o.Discount,
                Total = o.Total,
                Status = o.Status,
                PaymentQRUrl = o.PaymentQRUrl,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt,
                Items = o.Items.Select(i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    EventId = i.EventId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal
                }).ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<OrderSummaryResponse>> GetMyOrdersAsync(int tenantId, int userId)
    {
        return await _db.Orders
            .Where(o => o.TenantId == tenantId && o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryResponse
            {
                Id = o.Id,
                Total = o.Total,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt
            })
            .ToListAsync();
    }

    public async Task<ConfirmPaymentResponse?> ConfirmPaymentAsync(int tenantId, int userId, int orderId)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Product)
            .Include(o => o.Items)
            .ThenInclude(i => i.Event)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.TenantId == tenantId && o.UserId == userId);

        if (order == null)
        {
            return null;
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException("Order is not pending.");
        }

        var tickets = new List<Ticket>();

        await using var transaction = await _db.Database.BeginTransactionAsync();

        foreach (var item in order.Items)
        {
            if (item.ProductId.HasValue)
            {
                var product = item.Product;
                if (product == null)
                {
                    throw new InvalidOperationException("Product not found.");
                }

                if (product.Stock < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient stock for product.");
                }

                product.Stock -= item.Quantity;
            }

            if (item.EventId.HasValue)
            {
                var eventEntity = item.Event;
                if (eventEntity == null)
                {
                    throw new InvalidOperationException("Event not found.");
                }

                if (eventEntity.AvailableTickets < item.Quantity)
                {
                    throw new InvalidOperationException("Insufficient tickets for event.");
                }

                eventEntity.AvailableTickets -= item.Quantity;

                for (var i = 0; i < item.Quantity; i++)
                {
                    var code = Guid.NewGuid().ToString();
                    tickets.Add(new Ticket
                    {
                        OrderId = order.Id,
                        EventId = eventEntity.Id,
                        UserId = order.UserId,
                        Code = code,
                        QRCodeUrl = QRHelper.GenerateQRCodeBase64(code),
                        Status = TicketStatus.Active,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        order.Status = OrderStatus.Paid;
        order.PaidAt = DateTime.UtcNow;

        if (tickets.Count > 0)
        {
            _db.Tickets.AddRange(tickets);
        }

        await _db.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ConfirmPaymentResponse
        {
            Success = true,
            Tickets = tickets.Select(t => new ConfirmPaymentTicketResponse
            {
                Id = t.Id,
                Code = t.Code,
                QRCodeUrl = t.QRCodeUrl
            }).ToList()
        };
    }
}
