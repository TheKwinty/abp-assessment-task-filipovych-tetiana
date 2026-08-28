using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.Api.Contracts.Halls;

public sealed record HallResponse(
    Guid Id,
    string Name,
    int Capacity,
    decimal BaseHourlyRate,
    IReadOnlyList<ServiceOfferingResponse> Services)
{
    public static HallResponse FromHall(Hall hall)
    {
        ArgumentNullException.ThrowIfNull(hall);

        var services = hall.ServiceOfferings
            .OrderBy(serviceOffering => serviceOffering.Name, StringComparer.Ordinal)
            .ThenBy(serviceOffering => serviceOffering.Id)
            .Select(ServiceOfferingResponse.FromServiceOffering)
            .ToList()
            .AsReadOnly();

        return new HallResponse(
            hall.Id,
            hall.Name,
            hall.Capacity,
            hall.BaseHourlyRate,
            services);
    }
}
