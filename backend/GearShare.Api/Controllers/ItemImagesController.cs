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

    [HttpDelete]
[Authorize(Roles = "OWNER,ADMIN")]
public async Task<IActionResult> DeleteByUrl(Guid itemId, [FromQuery] string url, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(url))
        return BadRequest("Missing 'url' query parameter.");

    // DEBUG: Log the raw URL we received
    Console.WriteLine($"[DELETE] Raw URL received: '{url}'");
    Console.WriteLine($"[DELETE] URL type: {url.GetType().Name}");
    Console.WriteLine($"[DELETE] URL length: {url.Length}");

    // 1) Load item & authorize
    var item = await _db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == itemId, ct);
    if (item is null) return NotFound("Item not found.");
    if (!IsOwnerOrAdmin(item.OwnerId)) return Forbid();

    // 2) Extract filename from URL - try multiple strategies
    string fileName = null;
    
    // Strategy 1: Standard URI parsing
    try
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        fileName = Path.GetFileName(uri.IsAbsoluteUri ? uri.AbsolutePath : uri.ToString());
        Console.WriteLine($"[DELETE] Strategy 1 - Extracted fileName: '{fileName}'");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[DELETE] Strategy 1 failed: {ex.Message}");
    }

    // Strategy 2: If that failed or gave weird result, try simple string parsing
    if (string.IsNullOrEmpty(fileName) || fileName.Contains("GearShare") || fileName.Length > 100)
    {
        Console.WriteLine($"[DELETE] Strategy 1 result invalid, trying Strategy 2");
        // Try to find the last / and get everything after it
        var lastSlash = url.LastIndexOf('/');
        if (lastSlash >= 0 && lastSlash < url.Length - 1)
        {
            fileName = url.Substring(lastSlash + 1);
            Console.WriteLine($"[DELETE] Strategy 2 - Extracted fileName: '{fileName}'");
        }
    }

    if (string.IsNullOrEmpty(fileName) || fileName.Contains("GearShare"))
    {
        Console.WriteLine($"[DELETE] All strategies failed. Trying to match by partial URL");
        // Strategy 3: Just try to find ANY image that contains part of the URL
        var allImages = await _db.ItemImages.Where(ii => ii.ItemId == itemId).ToListAsync(ct);
        Console.WriteLine($"[DELETE] Found {allImages.Count} images for item");
        
        // If there's only one image, just delete it
        if (allImages.Count == 1)
        {
            var dbImage = allImages[0];
            Console.WriteLine($"[DELETE] Only one image found, deleting it: {dbImage.FileName}");
            
            try { await _imageStorage.DeleteAsync(dbImage.RelativePath); } 
            catch (Exception ex) { Console.WriteLine($"Failed to delete file: {ex.Message}"); }
            
            _db.ItemImages.Remove(dbImage);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
        
        return BadRequest($"Invalid image URL: '{url}'");
    }

    Console.WriteLine($"[DELETE] Final fileName to search: '{fileName}'");

    // 3) Find the DB record by filename
    var imageRecord = await _db.ItemImages
        .FirstOrDefaultAsync(ii => ii.ItemId == itemId && ii.FileName == fileName, ct);

    if (imageRecord == null)
    {
        // Try matching by RelativePath instead
        imageRecord = await _db.ItemImages
            .FirstOrDefaultAsync(ii => ii.ItemId == itemId && ii.RelativePath.Contains(fileName), ct);
    }
    
    if (imageRecord == null)
    {
        var allImages = await _db.ItemImages.Where(ii => ii.ItemId == itemId).ToListAsync(ct);
        Console.WriteLine($"[DELETE] Image not found. Available images:");
        foreach (var img in allImages)
        {
            Console.WriteLine($"  - {img.FileName}");
        }
        return NotFound($"Image not found. Searched for: '{fileName}'");
    }

    // 4) Delete the physical file
    try
    {
        await _imageStorage.DeleteAsync(imageRecord.RelativePath);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to delete file {imageRecord.RelativePath}: {ex.Message}");
    }

    // 5) Remove from database
    _db.ItemImages.Remove(imageRecord);
    await _db.SaveChangesAsync(ct);

    Console.WriteLine($"[DELETE] Successfully deleted image: {imageRecord.FileName}");
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
