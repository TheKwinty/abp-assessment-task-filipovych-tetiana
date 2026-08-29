using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.Api.Contracts.Bookings;

public sealed record BookingResponse(
    Guid Id,
    Guid HallId,
    string HallName,
    int AttendeeCount,
    DateTimeOffset Start,
    DateTimeOffset End,
    int DurationHours,
    IReadOnlyList<BookedServiceResponse> Services,
    decimal TotalPrice,
    DateTimeOffset CreatedAt)
{
    public static BookingResponse FromBooking(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        var services = booking.BookedServices
            .OrderBy(bookedService => bookedService.Name, StringComparer.Ordinal)
            .ThenBy(bookedService => bookedService.SourceServiceOfferingId)
            .Select(BookedServiceResponse.FromBookedService)
            .ToList()
            .AsReadOnly();

        return new BookingResponse(
            booking.Id,
            booking.HallId,
            booking.HallName,
            booking.AttendeeCount,
            booking.StartAt,
            booking.EndAt,
            checked((int)booking.Duration.TotalHours),
            services,
            booking.TotalPrice,
            booking.CreatedAt);
    }
}
