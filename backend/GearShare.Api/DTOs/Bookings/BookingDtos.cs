using GearShare.Api.Domain.Enums;

namespace GearShare.Api.DTOs.Bookings;

public record CreateBookingRequest(Guid ListingId, DateOnly StartDate, DateOnly EndDate);

public record BookingDto(
    Guid Id,
    Guid ListingId,
    string ListingTitle,
    Guid RenterId,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalPrice,
    BookingStatus Status
);

public record UpdateBookingStatusRequest(string Status); // ACCEPTED | REJECTED | CANCELLED
