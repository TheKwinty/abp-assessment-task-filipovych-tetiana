using ConferenceRooms.Core.Entities;
using ConferenceRooms.Core.Pricing;

namespace ConferenceRooms.UnitTests.Pricing;

public sealed class RentalPriceCalculatorTests
{
    private static readonly TimeSpan BookingOffset = TimeSpan.FromHours(2);

    [Theory]
    [InlineData(6, 900)]
    [InlineData(9, 1000)]
    [InlineData(12, 1150)]
    [InlineData(14, 1000)]
    [InlineData(18, 800)]
    [InlineData(22, 800)]
    public void Calculate_ForOneHourAtTariffBoundary_ReturnsExpectedPrice(
        int startHour,
        int expectedPrice)
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        var result = calculator.Calculate(
            hall,
            CreateStart(startHour),
            TimeSpan.FromHours(1),
            Array.Empty<Guid>());

        Assert.Equal((decimal)expectedPrice, result);
    }

    [Fact]
    public void Calculate_WithNullHall_ThrowsArgumentNullException()
    {
        var calculator = new RentalPriceCalculator();

        Assert.Throws<ArgumentNullException>(
            () => calculator.Calculate(
                null!,
                CreateStart(10),
                TimeSpan.FromHours(1),
                Array.Empty<Guid>()));
    }

    [Fact]
    public void Calculate_WithNullSelectedServiceIds_ThrowsArgumentNullException()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        Assert.Throws<ArgumentNullException>(
            () => calculator.Calculate(
                hall,
                CreateStart(10),
                TimeSpan.FromHours(1),
                null!));
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(-TimeSpan.TicksPerHour)]
    public void Calculate_WhenDurationIsNotPositive_ThrowsArgumentOutOfRangeException(
        long durationTicks)
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.Calculate(
                hall,
                CreateStart(10),
                TimeSpan.FromTicks(durationTicks),
                Array.Empty<Guid>()));
    }

    [Fact]
    public void Calculate_WhenDurationIsNotWholeHours_ThrowsArgumentException()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        Assert.Throws<ArgumentException>(
            () => calculator.Calculate(
                hall,
                CreateStart(10),
                TimeSpan.FromMinutes(90),
                Array.Empty<Guid>()));
    }

    [Theory]
    [InlineData(15, 0, 0L)]
    [InlineData(45, 0, 0L)]
    [InlineData(30, 1, 0L)]
    [InlineData(30, 0, 1L)]
    public void Calculate_WhenStartIsNotThirtyMinuteAligned_ThrowsArgumentException(
        int minute,
        int second,
        long additionalTicks)
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);
        var start = CreateStart(10, minute, second).AddTicks(additionalTicks);

        Assert.Throws<ArgumentException>(
            () => calculator.Calculate(
                hall,
                start,
                TimeSpan.FromHours(1),
                Array.Empty<Guid>()));
    }

    [Fact]
    public void Calculate_WhenBookingStartsAtHalfHour_SumsTariffSegments()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(2000m);

        var result = calculator.Calculate(
            hall,
            CreateStart(10, 30),
            TimeSpan.FromHours(2),
            Array.Empty<Guid>());

        Assert.Equal(4150m, result);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(23)]
    public void Calculate_WhenStartIsOutsideOperatingHours_ThrowsArgumentOutOfRangeException(
        int startHour)
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.Calculate(
                hall,
                CreateStart(startHour),
                TimeSpan.FromHours(1),
                Array.Empty<Guid>()));
    }

    [Fact]
    public void Calculate_WhenBookingEndsAfterClosing_ThrowsArgumentOutOfRangeException()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.Calculate(
                hall,
                CreateStart(22),
                TimeSpan.FromHours(2),
                Array.Empty<Guid>()));
    }

    [Fact]
    public void Calculate_WhenBookingCrossesIntoNextDay_ThrowsArgumentOutOfRangeException()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => calculator.Calculate(
                hall,
                CreateStart(22),
                TimeSpan.FromHours(3),
                Array.Empty<Guid>()));
    }

    [Fact]
    public void Calculate_WhenBookingEndsAtClosingTime_ReturnsPrice()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        var result = calculator.Calculate(
            hall,
            CreateStart(22),
            TimeSpan.FromHours(1),
            Array.Empty<Guid>());

        Assert.Equal(800m, result);
    }

    [Fact]
    public void Calculate_WhenBookingCrossesTariffPeriods_SumsSegmentPrices()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(2000m);

        var result = calculator.Calculate(
            hall,
            CreateStart(8),
            TimeSpan.FromHours(5),
            Array.Empty<Guid>());

        Assert.Equal(10100m, result);
    }

    [Fact]
    public void Calculate_ForFullOperatingDay_SumsAllTariffPeriods()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        var result = calculator.Calculate(
            hall,
            CreateStart(6),
            TimeSpan.FromHours(17),
            Array.Empty<Guid>());

        Assert.Equal(16000m, result);
    }

    [Fact]
    public void Calculate_WithEmptyServiceSelection_ReturnsHallTimeSubtotal()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        var result = calculator.Calculate(
            hall,
            CreateStart(9),
            TimeSpan.FromHours(1),
            Array.Empty<Guid>());

        Assert.Equal(1000m, result);
    }

    [Fact]
    public void Calculate_WithOneSelectedService_AddsServicePrice()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);
        var projector = AddServiceOffering(hall, "Projector", 500m);

        var result = calculator.Calculate(
            hall,
            CreateStart(9),
            TimeSpan.FromHours(1),
            [projector.Id]);

        Assert.Equal(1500m, result);
    }

    [Fact]
    public void Calculate_WithMultipleSelectedServices_AddsEachServicePrice()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(2000m);
        var projector = AddServiceOffering(hall, "Projector", 500m);
        var wifi = AddServiceOffering(hall, "Wi-Fi", 300m);

        var result = calculator.Calculate(
            hall,
            CreateStart(8),
            TimeSpan.FromHours(5),
            [projector.Id, wifi.Id]);

        Assert.Equal(10900m, result);
    }

    [Fact]
    public void Calculate_ForLongBooking_AddsServicePriceOnlyOnce()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(0m);
        var projector = AddServiceOffering(hall, "Projector", 500m);

        var result = calculator.Calculate(
            hall,
            CreateStart(18),
            TimeSpan.FromHours(5),
            [projector.Id]);

        Assert.Equal(500m, result);
    }

    [Fact]
    public void Calculate_WithUnknownServiceId_ThrowsArgumentException()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);

        Assert.Throws<ArgumentException>(
            () => calculator.Calculate(
                hall,
                CreateStart(9),
                TimeSpan.FromHours(1),
                [Guid.NewGuid()]));
    }

    [Fact]
    public void Calculate_WithDuplicateServiceId_ThrowsArgumentException()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(1000m);
        var projector = AddServiceOffering(hall, "Projector", 500m);

        Assert.Throws<ArgumentException>(
            () => calculator.Calculate(
                hall,
                CreateStart(9),
                TimeSpan.FromHours(1),
                [projector.Id, projector.Id]));
    }

    [Fact]
    public void Calculate_WhenTotalIsMidpoint_RoundsAwayFromZero()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(0.10m);

        var result = calculator.Calculate(
            hall,
            CreateStart(12),
            TimeSpan.FromHours(1),
            Array.Empty<Guid>());

        Assert.Equal(0.12m, result);
    }

    [Fact]
    public void Calculate_WhenSegmentsHaveFractionalCents_RoundsOnlyFinalTotal()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(0.05m);

        var result = calculator.Calculate(
            hall,
            CreateStart(8),
            TimeSpan.FromHours(5),
            Array.Empty<Guid>());

        Assert.Equal(0.25m, result);
    }

    [Fact]
    public void Calculate_WithFractionalServicePrices_RoundsOnlyCombinedTotal()
    {
        var calculator = new RentalPriceCalculator();
        var hall = CreateHall(0m);
        var firstService = AddServiceOffering(hall, "First service", 0.005m);
        var secondService = AddServiceOffering(hall, "Second service", 0.005m);

        var result = calculator.Calculate(
            hall,
            CreateStart(9),
            TimeSpan.FromHours(1),
            [firstService.Id, secondService.Id]);

        Assert.Equal(0.01m, result);
    }

    private static Hall CreateHall(decimal baseHourlyRate)
    {
        return new Hall(Guid.NewGuid(), "Hall A", 20, baseHourlyRate);
    }

    private static ServiceOffering AddServiceOffering(
        Hall hall,
        string name,
        decimal price)
    {
        var serviceOffering = new ServiceOffering(Guid.NewGuid(), name, price);
        hall.AddServiceOffering(serviceOffering);

        return serviceOffering;
    }

    private static DateTimeOffset CreateStart(int hour, int minute = 0, int second = 0)
    {
        return new DateTimeOffset(2026, 1, 15, hour, minute, second, BookingOffset);
    }
}
