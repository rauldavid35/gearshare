using System.ComponentModel.DataAnnotations;
using GearShare.Api.Domain.Enums;

namespace GearShare.Api.Domain.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ListingId { get; set; }
    public Listing Listing { get; set; } = default!;

    public Guid RenterId { get; set; }

    [Required] public DateOnly StartDate { get; set; }
    [Required] public DateOnly EndDate { get; set; }

    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
}
