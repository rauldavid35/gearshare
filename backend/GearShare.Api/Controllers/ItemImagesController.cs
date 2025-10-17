using System.Security.Claims;
using AutoMapper;
using GearShare.Api.Data;
using GearShare.Api.Domain.Entities;
using GearShare.Api.Models;
using GearShare.Api.Services;
using GearShare.Api.Utils; // ToAbsoluteContentUrl
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace GearShare.Api.Controllers;

[ApiController]
[Route("api/items/{itemId:guid}/images")]
public class ItemImagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IImageStorage _imageStorage;

    private readonly IWebHostEnvironment _env;   // ⬅️ add this

    public ItemImagesController(
        AppDbContext db,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        IImageStorage imageStorage,
        IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
        _imageStorage = imageStorage;
        _env = env; 
    }

    // POST /api/items/{itemId}/images  (upload one file via form-data: file)
    [HttpPost]
    [Authorize(Roles = "OWNER,ADMIN")]
    [RequestSizeLimit(10_000_000)] // ~10MB
    public async Task<ActionResult<object>> Upload(Guid itemId, IFormFile file, CancellationToken ct)
    {
        var item = await _db.Items.Include(i => i.Images).FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item is null) return NotFound();

        if (!IsOwnerOrAdmin(item.OwnerId)) return Forbid();
        if (file is null || file.Length == 0) return BadRequest("Empty file.");

        var (relative, fileName) = await _imageStorage.SaveItemImageAsync(itemId, file);

        var sortOrder = (item.Images?.Count ?? 0) + 1;
        var img = new ItemImage
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            FileName = fileName,
            RelativePath = relative,
            SortOrder = sortOrder
        };

        _db.ItemImages.Add(img);
        await _db.SaveChangesAsync(ct);

        return Ok(new
        {
            id = img.Id,
            url = Request.ToAbsoluteContentUrl(img.RelativePath)
        });
    }

    [HttpDelete] // DELETE /api/items/{itemId}/images?url=...
[Authorize(Roles = "OWNER,ADMIN")]
public async Task<IActionResult> DeleteByUrl(Guid itemId, [FromQuery] string url, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(url))
        return BadRequest("Missing 'url' query parameter.");

    // 1) Load item & authorize (owner or admin)
    var item = await _db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == itemId, ct);
    if (item is null) return NotFound("Item not found.");
    if (!IsOwnerOrAdmin(item.OwnerId)) return Forbid();

    // 2) Extract file name from absolute or relative URL
    string fileName;
    try
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        fileName = Path.GetFileName(uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString());
    }
    catch
    {
        fileName = Path.GetFileName(url);
    }
    if (string.IsNullOrEmpty(fileName))
        return BadRequest("Invalid image URL.");

    // 3) Try to delete the expected relative path first
    var expectedRel = $"/uploads/items/{itemId}/{fileName}";
    try { await _imageStorage.DeleteAsync(expectedRel); } catch { /* ignore */ }

    // 4) If file still exists, fallback: search by file name anywhere under /uploads/items
    string uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "items");
    try
    {
        if (Directory.Exists(uploadsRoot))
        {
            // Find all matches by exact filename (case-insensitive)
            var matches = Directory.GetFiles(uploadsRoot, fileName, SearchOption.AllDirectories);
            foreach (var full in matches)
            {
                try { System.IO.File.Delete(full); } catch { /* ignore */ }
            }
        }
    }
    catch { /* ignore */ }

    // 5) Remove DB row if present (optional best-effort)
    var lowerName = fileName.ToLower();
    var dbImage = await _db.ItemImages
        .FirstOrDefaultAsync(ii => ii.ItemId == itemId && ii.FileName.ToLower() == lowerName, ct);

    if (dbImage != null)
    {
        _db.ItemImages.Remove(dbImage);
        await _db.SaveChangesAsync(ct);
    }

    return NoContent();
}

// helper same as in ItemsController
private bool IsOwnerOrAdmin(Guid ownerId)
{
    if (User.Identity?.IsAuthenticated != true) return false;
    if (User.IsInRole("ADMIN")) return true;
    var uidStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    return Guid.TryParse(uidStr, out var uid) && uid == ownerId;
}



    private Guid GetUserId()
    {
        var sid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sid!);
    }
    
}
