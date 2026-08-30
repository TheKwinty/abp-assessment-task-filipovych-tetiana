using ConferenceRooms.Core.Scheduling;

namespace ConferenceRooms.UnitTests.Scheduling;

public sealed class BookingTimeRulesTests
{
    private static readonly DateTimeOffset DefaultDate =
        new(2030, 10, 1, 0, 0, 0, TimeSpan.FromHours(3));

    [Theory]
    [InlineData(6, 0, 1, 7, 0)]
    [InlineData(6, 30, 1, 7, 30)]
    [InlineData(10, 0, 2, 12, 0)]
    [InlineData(10, 30, 2, 12, 30)]
    [InlineData(21, 30, 1, 22, 30)]
    [InlineData(22, 0, 1, 23, 0)]
    [InlineData(6, 0, 17, 23, 0)]
    public void TryCreateWindow_WithValidSchedule_ReturnsExpectedEnd(
        int startHour,
        int startMinute,
        int durationHours,
        int expectedEndHour,
        int expectedEndMinute)
    {
        var start = DefaultDate.AddHours(startHour).AddMinutes(startMinute);
        var expectedEnd = DefaultDate
            .AddHours(expectedEndHour)
            .AddMinutes(expectedEndMinute);

        var created = BookingTimeRules.TryCreateWindow(
            start,
            durationHours,
            out var end);

        Assert.True(created);
        Assert.Equal(expectedEnd, end);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TryCreateWindow_WithNonPositiveDuration_ReturnsFalse(int durationHours)
    {
        var created = BookingTimeRules.TryCreateWindow(
            DefaultDate.AddHours(10),
            durationHours,
            out _);

        Assert.False(created);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(23)]
    public void TryCreateWindow_WithStartOutsideOperatingHours_ReturnsFalse(int startHour)
    {
        var created = BookingTimeRules.TryCreateWindow(
            DefaultDate.AddHours(startHour),
            1,
            out _);

        Assert.False(created);
    }

    [Theory]
    [InlineData(15, 0, 0L)]
    [InlineData(45, 0, 0L)]
    [InlineData(30, 1, 0L)]
    [InlineData(30, 0, 1L)]
    public void TryCreateWindow_WithStartOutsideThirtyMinuteBoundary_ReturnsFalse(
        int minute,
        int second,
        long additionalTicks)
    {
        var start = DefaultDate
            .AddHours(10)
            .AddMinutes(minute)
            .AddSeconds(second)
            .AddTicks(additionalTicks);

        var created = BookingTimeRules.TryCreateWindow(start, 1, out _);

        Assert.False(created);
    }

    [Theory]
    [InlineData(22, 0, 2)]
    [InlineData(22, 30, 1)]
    public void TryCreateWindow_EndingAfterClosing_ReturnsFalse(
        int startHour,
        int startMinute,
        int durationHours)
    {
        var created = BookingTimeRules.TryCreateWindow(
            DefaultDate.AddHours(startHour).AddMinutes(startMinute),
            durationHours,
            out _);

        Assert.False(created);
    }

    [Fact]
    public void TryCreateWindow_CrossingCalendarDay_ReturnsFalse()
    {
        var created = BookingTimeRules.TryCreateWindow(
            DefaultDate.AddHours(6),
            18,
            out _);

        Assert.False(created);
    }

    [Fact]
    public void TryCreateWindow_WithOversizedDuration_ReturnsFalseWithoutThrowing()
    {
        var created = BookingTimeRules.TryCreateWindow(
            DefaultDate.AddHours(10),
            int.MaxValue,
            out _);

        Assert.False(created);
    }

    [Fact]
    public void TryCreateWindow_NearDateTimeOffsetMaximum_ReturnsFalseWithoutThrowing()
    {
        var start = new DateTimeOffset(
            9999,
            12,
            31,
            22,
            0,
            0,
            TimeSpan.Zero);

        var created = BookingTimeRules.TryCreateWindow(
            start,
            2,
            out _);

        Assert.False(created);
    }
}
