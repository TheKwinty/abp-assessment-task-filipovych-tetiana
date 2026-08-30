namespace ConferenceRooms.Core.Scheduling;

public static class BookingTimeRules
{
    private const int MaximumDurationHours = 17;
    private const long StartIntervalTicks = TimeSpan.TicksPerMinute * 30;

    private static readonly TimeSpan OpeningTime = TimeSpan.FromHours(6);
    private static readonly TimeSpan ClosingTime = TimeSpan.FromHours(23);

    public static bool TryCreateWindow(
        DateTimeOffset start,
        int durationHours,
        out DateTimeOffset end)
    {
        end = default;

        if (durationHours <= 0
            || durationHours > MaximumDurationHours
            || start.Ticks % StartIntervalTicks != 0
            || start.TimeOfDay < OpeningTime
            || start.TimeOfDay >= ClosingTime)
        {
            return false;
        }

        try
        {
            end = start.AddHours(durationHours);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        return end.Date == start.Date
            && end.TimeOfDay <= ClosingTime;
    }
}
