using System.ComponentModel.DataAnnotations;

namespace ConferenceRooms.Api.Contracts.Bookings;

public sealed class CreateBookingRequest
{
    [Required]
    public Guid? HallId { get; init; }

    [Required]
    public DateTimeOffset? Start { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? DurationHours { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? AttendeeCount { get; init; }

    [Required]
    public IReadOnlyList<Guid>? ServiceOfferingIds { get; init; }
}
