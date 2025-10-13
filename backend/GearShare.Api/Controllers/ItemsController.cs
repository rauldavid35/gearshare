using System.Security.Claims;
using AutoMapper;
using GearShare.Api.Data;
using GearShare.Api.Domain.Entities;
using GearShare.Api.Domain.Enums;
using GearShare.Api.DTOs.Items;
using GearShare.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearShare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;

    public ItemsController(AppDbContext db, IMapper mapper, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
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
            query = query.Where(i => i.Title.ToLower().Contains(ql) || (i.Description != null && i.Description.ToLower().Contains(ql)));
        }

        if (cat.HasValue)
        {
            var catEnum = (ItemCategory)cat.Value;
            query = query.Where(i => i.Category == catEnum);
        }

        var items = await query.OrderByDescending(i => i.Id).ToListAsync(ct);
        return Ok(_mapper.Map<List<ItemDto>>(items));
    }

    // GET /api/items/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemDto>> GetOne(Guid id, CancellationToken ct)
    {
        var item = await _db.Items
            .Include(i => i.Images)
            .Include(i => i.Listings)
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (item is null) return NotFound();
        return Ok(_mapper.Map<ItemDto>(item));
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

        var dto = _mapper.Map<ItemDto>(entity);
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
        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item is null) return NotFound();

        if (!IsOwnerOrAdmin(item.OwnerId))
            return Forbid();

        _db.Items.Remove(item);
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
