using System.ComponentModel.DataAnnotations;

namespace ConferenceRooms.Api.Contracts.Halls;

public sealed class AvailabilitySearchRequest
{
    [Required]
    public DateTimeOffset? Start { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? DurationHours { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? Capacity { get; init; }
}
