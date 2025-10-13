using GearShare.Api.Domain.Enums;
public record CreateItemRequest(string Title, string? Description, ItemCategory Category, string Condition);
public record UpdateItemRequest(string Title, string? Description, ItemCategory Category, string Condition);
