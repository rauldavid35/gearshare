using System.Security.Claims;
using AutoMapper;
using GearShare.Api.Data;
using GearShare.Api.Domain.Entities;
using GearShare.Api.DTOs.Listings;
using GearShare.Api.Models;
using GearShare.Api.Utils; // <-- absolute URL helper
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearShare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ListingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public ListingsController(AppDbContext db, IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
    }

    // GET /api/listings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ListingDto>>> GetAll([FromQuery] Guid? itemId, CancellationToken ct)
    {
        var q = _db.Listings.AsNoTracking()
            .Include(l => l.Item)
                .ThenInclude(i => i.Images)
            .AsQueryable();

        if (itemId.HasValue) q = q.Where(l => l.ItemId == itemId.Value);

        var list = await q.OrderByDescending(l => l.Id).ToListAsync(ct);
        var dtos = _mapper.Map<List<ListingDto>>(list);

        // Make cover image absolute (if present)
        for (int i = 0; i < dtos.Count; i++)
            dtos[i] = dtos[i] with { CoverImage = Request.ToAbsoluteContentUrl(dtos[i].CoverImage) };

        return Ok(dtos);
    }

    // GET /api/listings/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ListingDto>> GetOne(Guid id, CancellationToken ct)
    {
        var entity = await _db.Listings.AsNoTracking()
            .Include(l => l.Item)
                .ThenInclude(i => i.Images)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (entity is null) return NotFound();

        var dto = _mapper.Map<ListingDto>(entity);
        dto = dto with { CoverImage = Request.ToAbsoluteContentUrl(dto.CoverImage) };
        return Ok(dto);
    }

    // POST /api/listings
    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpPost]
    public async Task<ActionResult<ListingDto>> Create([FromBody] CreateListingRequest req, CancellationToken ct)
    {
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == req.ItemId, ct);
        if (item is null) return BadRequest("Item not found.");
        if (!IsOwnerOrAdmin(item.OwnerId)) return Forbid();

        var entity = _mapper.Map<Listing>(req);
        _db.Listings.Add(entity);
        await _db.SaveChangesAsync(ct);

        // Re-load with Item+Images so DTO has title/cover, then absolutize URL
        var withItem = await _db.Listings.AsNoTracking()
            .Include(l => l.Item).ThenInclude(i => i.Images)
            .FirstAsync(l => l.Id == entity.Id, ct);

        var dto = _mapper.Map<ListingDto>(withItem);
        dto = dto with { CoverImage = Request.ToAbsoluteContentUrl(dto.CoverImage) };

        return CreatedAtAction(nameof(GetOne), new { id = entity.Id }, dto);
    }

    // PUT /api/listings/{id}
    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateListingRequest req, CancellationToken ct)
    {
        var entity = await _db.Listings.Include(l => l.Item).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (entity is null) return NotFound();
        if (!IsOwnerOrAdmin(entity.Item.OwnerId)) return Forbid();

        _mapper.Map(req, entity);
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    // DELETE /api/listings/{id}
    [Authorize(Roles = "OWNER,ADMIN")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var entity = await _db.Listings.Include(l => l.Item).FirstOrDefaultAsync(l => l.Id == id, ct);
        if (entity is null) return NotFound();
        if (!IsOwnerOrAdmin(entity.Item.OwnerId)) return Forbid();

        _db.Listings.Remove(entity);
        await _db.SaveChangesAsync(ct);
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
