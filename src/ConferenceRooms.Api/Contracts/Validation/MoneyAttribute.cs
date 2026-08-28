using System.ComponentModel.DataAnnotations;

namespace ConferenceRooms.Api.Contracts.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class MoneyAttribute : ValidationAttribute
{
    private const decimal MaximumValue = 9999999999999999.99m;

    public MoneyAttribute()
        : base(
            "The {0} field must be a non-negative monetary value with at most " +
            "2 decimal places and no more than 9999999999999999.99.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is not decimal monetaryValue)
        {
            return false;
        }

        return monetaryValue >= 0m
            && monetaryValue <= MaximumValue
            && GetScale(monetaryValue) <= 2;
    }

    private static int GetScale(decimal value)
    {
        // Decimal stores its scale in bits 16-23 of the flags component.
        return (decimal.GetBits(value)[3] >> 16) & 0x7F;
    }
}
