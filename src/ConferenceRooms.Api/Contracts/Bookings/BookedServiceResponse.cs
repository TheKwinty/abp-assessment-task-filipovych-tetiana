using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.Api.Contracts.Bookings;

public sealed record BookedServiceResponse(
    Guid SourceServiceOfferingId,
    string Name,
    decimal Price)
{
    public static BookedServiceResponse FromBookedService(BookedService bookedService)
    {
        ArgumentNullException.ThrowIfNull(bookedService);

        return new BookedServiceResponse(
            bookedService.SourceServiceOfferingId,
            bookedService.Name,
            bookedService.Price);
    }
}
