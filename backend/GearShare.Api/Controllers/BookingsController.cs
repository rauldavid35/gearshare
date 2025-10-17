using System.Security.Claims;
using AutoMapper;
using GearShare.Api.Data;
using GearShare.Api.Domain.Entities;
using GearShare.Api.Domain.Enums;
using GearShare.Api.DTOs.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GearShare.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public BookingsController(AppDbContext db, IMapper mapper)
    {
        _db = db; _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = "RENTER,ADMIN")]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest req, CancellationToken ct)
    {
        if (req.EndDate < req.StartDate) return BadRequest("End before start.");

        var listing = await _db.Listings.Include(l => l.Item).FirstOrDefaultAsync(l => l.Id == req.ListingId, ct);
        if (listing is null || !listing.Active) return BadRequest("Listing not available.");

        // Disallow overlap with ACCEPTED bookings
        var overlap = await _db.Bookings.AnyAsync(b =>
            b.ListingId == listing.Id &&
            b.Status == BookingStatus.Accepted &&
            b.StartDate <= req.EndDate && req.StartDate <= b.EndDate, ct);
        if (overlap) return Conflict("Dates overlap an existing booking.");

        var days = (req.EndDate.DayNumber - req.StartDate.DayNumber) + 1;
        if (days <= 0) return BadRequest("EndDate must be >= StartDate.");

        var total = (decimal)days * listing.PricePerDay + listing.Deposit;

        var renterId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var booking = new Booking
        {
            ListingId = listing.Id,
            RenterId = renterId,
            StartDate = req.StartDate,
            EndDate = req.EndDate,
            TotalPrice = total,
            Status = BookingStatus.Pending
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync(ct);

        var dto = await _db.Bookings
            .Include(b => b.Listing).ThenInclude(l => l.Item)
            .Where(b => b.Id == booking.Id)
            .Select(b => _mapper.Map<BookingDto>(b))
            .FirstAsync(ct);

        return CreatedAtAction(nameof(GetOne), new { id = booking.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<BookingDto>> GetOne(Guid id, CancellationToken ct)
    {
        var b = await _db.Bookings
            .Include(x => x.Listing).ThenInclude(l => l.Item)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (b is null) return NotFound();
        return Ok(_mapper.Map<BookingDto>(b));
    }

    // Renter: own bookings
    [HttpGet("me")]
    [Authorize(Roles = "RENTER,ADMIN")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> Mine(CancellationToken ct)
    {
        var uid = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _db.Bookings
            .Include(b => b.Listing).ThenInclude(l => l.Item)
            .Where(b => b.RenterId == uid)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync(ct);
        return Ok(_mapper.Map<List<BookingDto>>(list));
    }

    // Owner: pending on their listings
    [HttpGet("owner")]
    [Authorize(Roles = "OWNER,ADMIN")]
    public async Task<ActionResult<IEnumerable<BookingDto>>> OwnerPending(CancellationToken ct)
    {
        var uid = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _db.Bookings
            .Include(b => b.Listing).ThenInclude(l => l.Item)
            .Where(b => b.Listing.Item.OwnerId == uid && b.Status == BookingStatus.Pending)
            .OrderByDescending(b => b.StartDate)
            .ToListAsync(ct);
        return Ok(_mapper.Map<List<BookingDto>>(list));
    }

    // Owner accepts/rejects/cancels
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "OWNER,ADMIN")]
    public async Task<IActionResult> SetStatus(Guid id, UpdateBookingStatusRequest req, CancellationToken ct)
    {
        var b = await _db.Bookings.Include(x => x.Listing).ThenInclude(l => l.Item)
                                  .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (b is null) return NotFound();

        var ownerId = b.Listing.Item.OwnerId;
        var uid = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (uid != ownerId && !User.IsInRole("ADMIN")) return Forbid();

        var s = req.Status.Trim().ToUpperInvariant();
        if (s is "ACCEPTED") b.Status = BookingStatus.Accepted;
        else if (s is "REJECTED") b.Status = BookingStatus.Rejected;
        else if (s is "CANCELLED") b.Status = BookingStatus.Cancelled;
        else return BadRequest("Unknown status.");

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
