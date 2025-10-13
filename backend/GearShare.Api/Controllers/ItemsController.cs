using System.Security.Claims;
using AutoMapper;
using GearShare.Api.Data;
using GearShare.Api.Domain.Entities;
using GearShare.Api.Domain.Enums;
using GearShare.Api.DTOs.Items;
using GearShare.Api.Models;
using GearShare.Api.Services;
using GearShare.Api.Utils; // ToAbsoluteContentUrl
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq; // for Enumerable.Empty

namespace GearShare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IImageStorage _imageStorage;

    public ItemsController(
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

    // GET /api/items?q=&cat=0
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> Get([FromQuery] string? q, [FromQuery] int? cat, CancellationToken ct)
    {
        var query = _db.Items
            .AsNoTracking()
            .Include(i => i.Images)
            .Include(i => i.Listings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.Trim().ToLower();
            query = query.Where(i =>
                i.Title.ToLower().Contains(ql) ||
                (i.Description != null && i.Description.ToLower().Contains(ql)));
        }

        if (cat.HasValue)
        {
            var catEnum = (ItemCategory)cat.Value;
            query = query.Where(i => i.Category == catEnum);
        }

        var items = await query.OrderByDescending(i => i.Id).ToListAsync(ct);
        var dtos = _mapper.Map<List<ItemDto>>(items);

        // Build a non-null list of absolute URLs
        for (int i = 0; i < dtos.Count; i++)
        {
            var imgs = (dtos[i].Images ?? Enumerable.Empty<string>())
                .Select(p => Request.ToAbsoluteContentUrl(p)!)
                .ToList();
            dtos[i] = dtos[i] with { Images = imgs };
        }

        return Ok(dtos);
    }

    // GET /api/items/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDto>> GetOne(Guid id, CancellationToken ct)
    {
        var item = await _db.Items
            .AsNoTracking()
            .Include(i => i.Images)
            .Include(i => i.Listings)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (item is null) return NotFound();

        var dto = _mapper.Map<ItemDto>(item);
        var imgs = (dto.Images ?? Enumerable.Empty<string>())
            .Select(p => Request.ToAbsoluteContentUrl(p)!)
            .ToList();
        dto = dto with { Images = imgs };

        return Ok(dto);
    }

    // POST /api/items
    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemRequest req, CancellationToken ct)
    {
        var uid = GetUserId();
        var entity = _mapper.Map<Item>(req);
        entity.OwnerId = uid;

        _db.Items.Add(entity);
        await _db.SaveChangesAsync(ct);

        var reloaded = await _db.Items
            .AsNoTracking()
            .Include(i => i.Images)
            .FirstAsync(i => i.Id == entity.Id, ct);

        var dto = _mapper.Map<ItemDto>(reloaded);
        var imgs = (dto.Images ?? Enumerable.Empty<string>())
            .Select(p => Request.ToAbsoluteContentUrl(p)!)
            .ToList();
        dto = dto with { Images = imgs };

        return CreatedAtAction(nameof(GetOne), new { id = entity.Id }, dto);
    }

    // PUT /api/items/{id}
    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemRequest req, CancellationToken ct)
    {
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item is null) return NotFound();

        if (!IsOwnerOrAdmin(item.OwnerId))
            return Forbid();

        _mapper.Map(req, item);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE /api/items/{id}
    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var item = await _db.Items
            .Include(i => i.Images)
            .Include(i => i.Listings)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (item is null) return NotFound();

        if (!IsOwnerOrAdmin(item.OwnerId))
            return Forbid();

        // 1) remember file paths before removing from DB
        var imagePaths = item.Images.Select(img => img.RelativePath).ToList();

        // 2) remove from DB (cascades to ItemImages & Listings)
        _db.Items.Remove(item);
        await _db.SaveChangesAsync(ct);

        // 3) delete physical files (ignore failures)
        foreach (var rel in imagePaths)
        {
            try { await _imageStorage.DeleteAsync(rel); } catch { /* optional log */ }
        }

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
