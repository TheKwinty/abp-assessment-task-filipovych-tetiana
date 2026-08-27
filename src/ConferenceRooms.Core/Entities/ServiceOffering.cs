namespace ConferenceRooms.Core.Entities;

public sealed class ServiceOffering
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public decimal Price { get; private set; }

    public ServiceOffering(Guid id, string name, decimal price)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Service offering ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Service offering name is required.", nameof(name));
        }

        if (price < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Service offering price cannot be negative.");
        }

        Id = id;
        Name = name.Trim();
        Price = price;
    }
}
