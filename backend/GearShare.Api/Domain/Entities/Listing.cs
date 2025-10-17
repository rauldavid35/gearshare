namespace GearShare.Api.Domain.Entities;

public class Listing
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = default!;
    public decimal PricePerDay { get; set; }
    public decimal Deposit { get; set; }
    public string? LocationCity { get; set; }
    public double? LocationLat { get; set; }
    public double? LocationLng { get; set; }
    public bool Active { get; set; } = true;

    // NEW: backref for owner views / integrity
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
