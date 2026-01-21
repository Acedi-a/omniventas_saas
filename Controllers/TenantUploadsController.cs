using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/tenant/uploads")]
[Authorize(Roles = "Admin")]
public class TenantUploadsController : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only image uploads are allowed." });
        }

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", $"tenant-{tenantId}");
        Directory.CreateDirectory(uploadsRoot);
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        var url = $"/uploads/tenant-{tenantId}/{fileName}";
        return Ok(new { url });
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var claim = User.FindFirst("TenantId")?.Value;
        return int.TryParse(claim, out tenantId);
    }
}
