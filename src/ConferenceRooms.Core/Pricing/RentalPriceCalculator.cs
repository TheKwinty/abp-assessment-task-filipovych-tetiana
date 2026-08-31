using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.Core.Pricing;

public sealed class RentalPriceCalculator
{
    private const long StartIntervalTicks = TimeSpan.TicksPerMinute * 30;

    private static readonly IReadOnlyList<TariffPeriod> TariffSchedule =
        Array.AsReadOnly<TariffPeriod>(
        [
            new(new TimeOnly(6, 0), new TimeOnly(9, 0), 0.90m),
            new(new TimeOnly(9, 0), new TimeOnly(12, 0), 1.00m),
            new(new TimeOnly(12, 0), new TimeOnly(14, 0), 1.15m),
            new(new TimeOnly(14, 0), new TimeOnly(18, 0), 1.00m),
            new(new TimeOnly(18, 0), new TimeOnly(23, 0), 0.80m),
        ]);

    public decimal Calculate(
        Hall hall,
        DateTimeOffset start,
        TimeSpan duration,
        IReadOnlyCollection<Guid> selectedServiceOfferingIds)
    {
        ArgumentNullException.ThrowIfNull(hall);
        ArgumentNullException.ThrowIfNull(selectedServiceOfferingIds);

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than zero.");
        }

        if (duration.Ticks % TimeSpan.TicksPerHour != 0)
        {
            throw new ArgumentException("Duration must represent complete hours.", nameof(duration));
        }

        if (start.TimeOfDay.Ticks % StartIntervalTicks != 0)
        {
            throw new ArgumentException(
                "Start must be aligned to a 30-minute boundary.",
                nameof(start));
        }

        var startTime = TimeOnly.FromTimeSpan(start.TimeOfDay);

        if (startTime < TariffSchedule[0].Start || startTime >= TariffSchedule[^1].End)
        {
            throw new ArgumentOutOfRangeException(nameof(start), "Start must be within operating hours.");
        }

        var end = start.Add(duration);
        var closingTime = CreateBoundary(start, TariffSchedule[^1].End);

        if (end.Date != start.Date || end > closingTime)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Booking must end within operating hours.");
        }

        var selectedServicesSubtotal = CalculateSelectedServicesSubtotal(
            hall,
            selectedServiceOfferingIds);
        var hallTimeSubtotal = 0m;

        // Each half-open tariff segment contributes only its overlapping duration,
        // which prorates bookings that cross one or more tariff boundaries.
        foreach (var tariffPeriod in TariffSchedule)
        {
            var tariffStart = CreateBoundary(start, tariffPeriod.Start);
            var tariffEnd = CreateBoundary(start, tariffPeriod.End);
            var segmentStart = start > tariffStart ? start : tariffStart;
            var segmentEnd = end < tariffEnd ? end : tariffEnd;

            if (segmentEnd <= segmentStart)
            {
                continue;
            }

            var segmentHours = (segmentEnd - segmentStart).Ticks
                / (decimal)TimeSpan.TicksPerHour;

            hallTimeSubtotal += hall.BaseHourlyRate
                * segmentHours
                * tariffPeriod.Multiplier;
        }

        var total = hallTimeSubtotal + selectedServicesSubtotal;

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    private static decimal CalculateSelectedServicesSubtotal(
        Hall hall,
        IReadOnlyCollection<Guid> selectedServiceOfferingIds)
    {
        var uniqueServiceIds = new HashSet<Guid>();
        var subtotal = 0m;

        foreach (var serviceOfferingId in selectedServiceOfferingIds)
        {
            if (!uniqueServiceIds.Add(serviceOfferingId))
            {
                throw new ArgumentException(
                    "Selected service offering IDs must be unique.",
                    nameof(selectedServiceOfferingIds));
            }

            var serviceOffering = hall.ServiceOfferings.FirstOrDefault(
                offering => offering.Id == serviceOfferingId);

            if (serviceOffering is null)
            {
                throw new ArgumentException(
                    "Every selected service offering must belong to the hall.",
                    nameof(selectedServiceOfferingIds));
            }

            subtotal += serviceOffering.Price;
        }

        return subtotal;
    }

    private static DateTimeOffset CreateBoundary(DateTimeOffset bookingStart, TimeOnly time)
    {
        return new DateTimeOffset(
            bookingStart.Date.Add(time.ToTimeSpan()),
            bookingStart.Offset);
    }
}
