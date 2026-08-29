using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.Api.Services;

public sealed class HallAvailabilityResult
{
    private HallAvailabilityResult(
        IReadOnlyList<Hall>? halls,
        HallAvailabilityFailure? failure)
    {
        Halls = halls;
        Failure = failure;
    }

    public IReadOnlyList<Hall>? Halls { get; }

    public HallAvailabilityFailure? Failure { get; }

    public static HallAvailabilityResult Succeeded(IReadOnlyList<Hall> halls)
    {
        ArgumentNullException.ThrowIfNull(halls);

        return new HallAvailabilityResult(halls, null);
    }

    public static HallAvailabilityResult Failed(HallAvailabilityFailure failure)
    {
        return new HallAvailabilityResult(null, failure);
    }
}
