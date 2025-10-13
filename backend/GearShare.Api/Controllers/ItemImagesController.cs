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

namespace GearShare.Api.Controllers;

[ApiController]
[Route("api/items/{itemId:guid}/images")]
public class ItemImagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IImageStorage _imageStorage;

    public ItemImagesController(
        AppDbContext db,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        IImageStorage imageStorage)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
        _imageStorage = imageStorage;
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

    // DELETE /api/items/{itemId}/images/{imageId}
    [HttpDelete("{imageId:guid}")]
    [Authorize(Roles = "OWNER,ADMIN")]
    public async Task<IActionResult> Delete(Guid itemId, Guid imageId, CancellationToken ct)
    {
        var img = await _db.ItemImages
            .Include(ii => ii.Item)
            .FirstOrDefaultAsync(ii => ii.Id == imageId && ii.ItemId == itemId, ct);

        if (img is null) return NotFound();
        if (!IsOwnerOrAdmin(img.Item.OwnerId)) return Forbid();

        var path = img.RelativePath;

        _db.ItemImages.Remove(img);
        await _db.SaveChangesAsync(ct);

        try { await _imageStorage.DeleteAsync(path); } catch { /* optional log */ }

        return NoContent();
    }

    private Guid GetUserId()
    {
        var sid = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sid!);
    }

    private bool IsOwnerOrAdmin(Guid ownerId)
    {
        if (User.Identity?.IsAuthenticated != true) return false;
        if (User.IsInRole("ADMIN")) return true;
        var uid = GetUserId();
        return uid == ownerId;
    }
}
