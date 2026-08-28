using System.ComponentModel.DataAnnotations;
using ConferenceRooms.Api.Contracts.Validation;

namespace ConferenceRooms.Api.Contracts.Halls;

public sealed class UpdateHallRequest : IValidatableObject
{
    [Required]
    [StringLength(200)]
    public string? Name { get; init; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? Capacity { get; init; }

    [Required]
    [Money]
    public decimal? BaseHourlyRate { get; init; }

    [Required]
    public IReadOnlyList<ServiceOfferingRequest?>? Services { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Services?.Any(service => service is null) == true)
        {
            yield return new ValidationResult(
                "Services cannot contain null items.",
                [nameof(Services)]);
        }
    }
}
