using ConferenceRooms.Core.Scheduling;

namespace ConferenceRooms.UnitTests.Scheduling;

public sealed class BookingTimeRulesTests
{
    private static readonly DateTimeOffset DefaultDate =
        new(2030, 10, 1, 0, 0, 0, TimeSpan.FromHours(3));

    [Theory]
    [InlineData(6, 1, 7)]
    [InlineData(10, 2, 12)]
    [InlineData(22, 1, 23)]
    [InlineData(6, 17, 23)]
    public void TryCreateWindow_WithValidSchedule_ReturnsExpectedEnd(
        int startHour,
        int durationHours,
        int expectedEndHour)
    {
        var start = DefaultDate.AddHours(startHour);
        var expectedEnd = DefaultDate.AddHours(expectedEndHour);

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

    [Fact]
    public void TryCreateWindow_WithPartialHourStart_ReturnsFalse()
    {
        var start = DefaultDate.AddHours(10).AddMinutes(30);

        var created = BookingTimeRules.TryCreateWindow(start, 1, out _);

        Assert.False(created);
    }

    [Fact]
    public void TryCreateWindow_WithSubSecondStart_ReturnsFalse()
    {
        var start = DefaultDate.AddHours(10).AddTicks(1);

        var created = BookingTimeRules.TryCreateWindow(start, 1, out _);

        Assert.False(created);
    }

    [Fact]
    public void TryCreateWindow_EndingAfterClosing_ReturnsFalse()
    {
        var created = BookingTimeRules.TryCreateWindow(
            DefaultDate.AddHours(22),
            2,
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
