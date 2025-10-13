using AutoMapper;
using GearShare.Api.Data;
using GearShare.Api.Domain.Entities;
using GearShare.Api.Models;
using GearShare.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GearShare.Api.Controllers;

[ApiController]
[Route("api/items/{itemId:guid}/images")]
public class ItemImagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IImageStorage _storage;
    private readonly UserManager<ApplicationUser> _userManager;

    public ItemImagesController(AppDbContext db, IImageStorage storage, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _storage = storage;
        _userManager = userManager;
    }

    // POST /api/items/{itemId}/images
    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpPost]
    [RequestSizeLimit(10_000_000)] // ~10 MB
    public async Task<ActionResult> Upload(Guid itemId, IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0) return BadRequest("No file.");

        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item is null) return NotFound();

        if (!IsOwnerOrAdmin(item.OwnerId))
            return Forbid();

        var (fileName, relativePath) = await _storage.SaveItemImageAsync(itemId, file, ct);

        var img = new ItemImage
        {
            ItemId = itemId,
            FileName = fileName,
            RelativePath = relativePath,
            SortOrder = 0
        };
        _db.ItemImages.Add(img);
        await _db.SaveChangesAsync(ct);

        return Ok(new { path = relativePath });
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
