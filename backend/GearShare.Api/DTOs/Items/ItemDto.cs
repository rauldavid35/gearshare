using GearShare.Api.Domain.Enums;
namespace GearShare.Api.DTOs.Items;
public record ItemDto(Guid Id, string Title, string? Description, ItemCategory Category,
                      string Condition, Guid OwnerId, double? RatingAvg,
                      List<string> Images,  // relative URLs
                      int ListingsCount);
