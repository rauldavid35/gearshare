namespace GearShare.Api.Domain.Entities;
public class ItemImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ItemId { get; set; }
    public Item Item { get; set; } = default!;
    public string FileName { get; set; } = default!;        // ex: "a1b2c3.jpg"
    public string RelativePath { get; set; } = default!;    // ex: "/uploads/items/a1b2c3.jpg"
    public int SortOrder { get; set; }
}
