using System.ComponentModel.DataAnnotations;
using GearShare.Api.Domain.Enums;

namespace GearShare.Api.Domain.Entities;
public class Item
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required, MaxLength(120)] public string Title { get; set; } = default!;
    [MaxLength(2000)] public string? Description { get; set; }
    public ItemCategory Category { get; set; }
    public string Condition { get; set; } = "GOOD"; // simplu
    public Guid OwnerId { get; set; }               // map la AspNetUsers.Id (Guid)
    public double? RatingAvg { get; set; }

    public ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
    public ICollection<Listing> Listings { get; set; } = new List<Listing>();

}
