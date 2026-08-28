using System.ComponentModel.DataAnnotations;
using ConferenceRooms.Api.Contracts.Validation;

namespace ConferenceRooms.Api.Contracts.Halls;

public sealed class ServiceOfferingRequest
{
    [Required]
    [StringLength(200)]
    public string? Name { get; init; }

    [Required]
    [Money]
    public decimal? Price { get; init; }
}
