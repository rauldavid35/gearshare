namespace GearShare.Api.DTOs.Listings;

public record ListingDto(
    Guid Id,
    Guid ItemId,
    string? ItemTitle,      // NEW
    string? CoverImage,     // NEW
    decimal PricePerDay,
    decimal Deposit,
    string? LocationCity,
    double? LocationLat,
    double? LocationLng,
    bool Active
);

// ⬇️ rămân EXACT cum le ai tu
public record CreateListingRequest(
    Guid ItemId,
    decimal PricePerDay,
    decimal Deposit,
    string? LocationCity,
    double? LocationLat,
    double? LocationLng,
    bool Active
);

public record UpdateListingRequest(
    decimal PricePerDay,
    decimal Deposit,
    string? LocationCity,
    double? LocationLat,
    double? LocationLng,
    bool Active
);
