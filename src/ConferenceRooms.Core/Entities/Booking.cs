namespace ConferenceRooms.Core.Entities;

public sealed class Booking
{
    private readonly List<BookedService> _bookedServices = [];

    public Guid Id { get; private set; }

    public Guid HallId { get; private set; }

    public string HallName { get; private set; }

    public int AttendeeCount { get; private set; }

    public DateTimeOffset StartAt { get; private set; }

    public DateTimeOffset EndAt { get; private set; }

    public decimal TotalPrice { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public TimeSpan Duration => EndAt - StartAt;

    public IReadOnlyCollection<BookedService> BookedServices { get; }

    private Booking()
    {
        HallName = null!;
        BookedServices = _bookedServices.AsReadOnly();
    }

    public Booking(
        Guid id,
        Guid hallId,
        string hallName,
        int attendeeCount,
        DateTimeOffset startAt,
        DateTimeOffset endAt,
        decimal totalPrice,
        DateTimeOffset createdAt,
        IEnumerable<BookedService> bookedServices)
        : this()
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Booking ID cannot be empty.", nameof(id));
        }

        if (hallId == Guid.Empty)
        {
            throw new ArgumentException("Hall ID cannot be empty.", nameof(hallId));
        }

        if (string.IsNullOrWhiteSpace(hallName))
        {
            throw new ArgumentException("Hall name is required.", nameof(hallName));
        }

        if (attendeeCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attendeeCount),
                "Attendee count must be greater than zero.");
        }

        if (endAt <= startAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endAt),
                "Booking end must be after its start.");
        }

        if (totalPrice < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(totalPrice),
                "Booking total price cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(bookedServices);

        var serviceSnapshots = bookedServices.ToList();

        if (serviceSnapshots.Any(bookedService => bookedService is null))
        {
            throw new ArgumentException(
                "Booked services cannot contain null items.",
                nameof(bookedServices));
        }

        Id = id;
        HallId = hallId;
        HallName = hallName.Trim();
        AttendeeCount = attendeeCount;
        StartAt = startAt;
        EndAt = endAt;
        TotalPrice = totalPrice;
        CreatedAt = createdAt;
        _bookedServices.AddRange(serviceSnapshots);
    }
}
