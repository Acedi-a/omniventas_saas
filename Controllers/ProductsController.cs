using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Products;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var products = await _productService.GetProductsAsync(tenantId);
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var product = await _productService.GetProductAsync(tenantId, id);
        if (product == null)
        {
            return NotFound(new { error = "Product not found." });
        }

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var product = await _productService.CreateProductAsync(tenantId, request);
        return Ok(product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductRequest request)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var product = await _productService.UpdateProductAsync(tenantId, id, request);
        if (product == null)
        {
            return NotFound(new { error = "Product not found." });
        }

        return Ok(product);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var deleted = await _productService.DeleteProductAsync(tenantId, id);
        if (!deleted)
        {
            return NotFound(new { error = "Product not found." });
        }

        return Ok(new { success = true });
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        return int.TryParse(tenantIdClaim, out tenantId);
    }
}
