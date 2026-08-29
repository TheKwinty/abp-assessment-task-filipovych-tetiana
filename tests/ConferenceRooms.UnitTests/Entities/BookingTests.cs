using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.UnitTests.Entities;

public sealed class BookingTests
{
    private static readonly DateTimeOffset DefaultStart =
        new(2026, 9, 5, 10, 0, 0, TimeSpan.FromHours(3));

    [Fact]
    public void Constructor_WithValidArguments_CreatesBooking()
    {
        var id = Guid.NewGuid();
        var hallId = Guid.NewGuid();
        var createdAt = DefaultStart.AddDays(-1);
        var bookedService = CreateBookedService();

        var booking = new Booking(
            id,
            hallId,
            "Hall A",
            40,
            DefaultStart,
            DefaultStart.AddHours(2),
            4800m,
            createdAt,
            [bookedService]);

        Assert.Equal(id, booking.Id);
        Assert.Equal(hallId, booking.HallId);
        Assert.Equal("Hall A", booking.HallName);
        Assert.Equal(40, booking.AttendeeCount);
        Assert.Equal(DefaultStart, booking.StartAt);
        Assert.Equal(DefaultStart.AddHours(2), booking.EndAt);
        Assert.Equal(4800m, booking.TotalPrice);
        Assert.Equal(createdAt, booking.CreatedAt);
        Assert.Same(bookedService, Assert.Single(booking.BookedServices));
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CreateBooking(id: Guid.Empty));
    }

    [Fact]
    public void Constructor_WithEmptyHallId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CreateBooking(hallId: Guid.Empty));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidHallName_ThrowsArgumentException(string? hallName)
    {
        Assert.Throws<ArgumentException>(() => CreateBooking(hallName: hallName!));
    }

    [Fact]
    public void Constructor_TrimsHallName()
    {
        var booking = CreateBooking(hallName: "  Hall A  ");

        Assert.Equal("Hall A", booking.HallName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveAttendeeCount_ThrowsArgumentOutOfRangeException(
        int attendeeCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBooking(attendeeCount: attendeeCount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithEndNotAfterStart_ThrowsArgumentOutOfRangeException(
        int endOffsetHours)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBooking(endAt: DefaultStart.AddHours(endOffsetHours)));
    }

    [Fact]
    public void Constructor_WithNegativeTotalPrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateBooking(totalPrice: -0.01m));
    }

    [Fact]
    public void Constructor_WithNullBookedServices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Booking(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Hall A",
            40,
            DefaultStart,
            DefaultStart.AddHours(2),
            4800m,
            DefaultStart.AddDays(-1),
            null!));
    }

    [Fact]
    public void Constructor_WithNullBookedService_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => CreateBooking(bookedServices: [null!]));
    }

    [Fact]
    public void Duration_ReturnsDifferenceBetweenEndAndStart()
    {
        var booking = CreateBooking(endAt: DefaultStart.AddHours(3));

        Assert.Equal(TimeSpan.FromHours(3), booking.Duration);
    }

    [Fact]
    public void BookedServices_CannotBeModifiedExternally()
    {
        var originalService = CreateBookedService();
        var booking = CreateBooking(bookedServices: [originalService]);
        var bookedServices = Assert.IsAssignableFrom<ICollection<BookedService>>(
            booking.BookedServices);

        Assert.True(bookedServices.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => bookedServices.Add(CreateBookedService()));
        Assert.Same(originalService, Assert.Single(booking.BookedServices));
    }

    private static Booking CreateBooking(
        Guid? id = null,
        Guid? hallId = null,
        string hallName = "Hall A",
        int attendeeCount = 40,
        DateTimeOffset? endAt = null,
        decimal totalPrice = 4800m,
        IEnumerable<BookedService>? bookedServices = null)
    {
        return new Booking(
            id ?? Guid.NewGuid(),
            hallId ?? Guid.NewGuid(),
            hallName,
            attendeeCount,
            DefaultStart,
            endAt ?? DefaultStart.AddHours(2),
            totalPrice,
            DefaultStart.AddDays(-1),
            bookedServices ?? [CreateBookedService()]);
    }

    private static BookedService CreateBookedService()
    {
        return new BookedService(Guid.NewGuid(), Guid.NewGuid(), "Projector", 500m);
    }
}
