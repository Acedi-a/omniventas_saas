using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Products;
using SaaSEventos.Models;

namespace SaaSEventos.Services;

public class ProductService
{
    private readonly AppDbContext _db;

    public ProductService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductResponse>> GetProductsAsync(int tenantId)
    {
        return await _db.Products
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProductResponse?> GetProductAsync(int tenantId, int id)
    {
        return await _db.Products
            .Where(p => p.TenantId == tenantId && p.Id == id)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductResponse> CreateProductAsync(int tenantId, CreateProductRequest request)
    {
        var product = new Product
        {
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            ImageUrl = request.ImageUrl,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt
        };
    }

    public async Task<ProductResponse?> UpdateProductAsync(int tenantId, int id, UpdateProductRequest request)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id);
        if (product == null)
        {
            return null;
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.ImageUrl = request.ImageUrl;

        await _db.SaveChangesAsync();

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            ImageUrl = product.ImageUrl,
            CreatedAt = product.CreatedAt
        };
    }

    public async Task<bool> DeleteProductAsync(int tenantId, int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == id);
        if (product == null)
        {
            return false;
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return true;
    }
}
