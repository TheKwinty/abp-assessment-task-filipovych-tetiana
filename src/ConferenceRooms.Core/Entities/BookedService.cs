namespace ConferenceRooms.Core.Entities;

public sealed class BookedService
{
    public Guid Id { get; private set; }

    public Guid SourceServiceOfferingId { get; private set; }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public BookedService(
        Guid id,
        Guid sourceServiceOfferingId,
        string name,
        decimal price)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Booked service ID cannot be empty.", nameof(id));
        }

        if (sourceServiceOfferingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Source service offering ID cannot be empty.",
                nameof(sourceServiceOfferingId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Booked service name is required.", nameof(name));
        }

        if (price < 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "Booked service price cannot be negative.");
        }

        Id = id;
        SourceServiceOfferingId = sourceServiceOfferingId;
        Name = name.Trim();
        Price = price;
    }
}
