using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.Api.Contracts.Halls;

public sealed record ServiceOfferingResponse(Guid Id, string Name, decimal Price)
{
    public static ServiceOfferingResponse FromServiceOffering(
        ServiceOffering serviceOffering)
    {
        ArgumentNullException.ThrowIfNull(serviceOffering);

        return new ServiceOfferingResponse(
            serviceOffering.Id,
            serviceOffering.Name,
            serviceOffering.Price);
    }
}
