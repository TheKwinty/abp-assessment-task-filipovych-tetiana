using System.ComponentModel.DataAnnotations;

namespace ConferenceRooms.Api.Contracts.Reports;

public sealed class BookingSummaryRequest : IValidatableObject
{
    [Required]
    public DateTimeOffset? From { get; init; }

    [Required]
    public DateTimeOffset? To { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From is DateTimeOffset from
            && To is DateTimeOffset to
            && from >= to)
        {
            yield return new ValidationResult(
                "From must be earlier than To.",
                [nameof(From), nameof(To)]);
        }
    }
}
