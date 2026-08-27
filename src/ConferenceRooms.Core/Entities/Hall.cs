namespace ConferenceRooms.Core.Entities;

public sealed class Hall
{
    private readonly List<ServiceOffering> _serviceOfferings = [];

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public int Capacity { get; private set; }

    public decimal BaseHourlyRate { get; private set; }

    public IReadOnlyCollection<ServiceOffering> ServiceOfferings { get; }

    public Hall(Guid id, string name, int capacity, decimal baseHourlyRate)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Hall ID cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Hall name is required.", nameof(name));
        }

        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Hall capacity must be greater than zero.");
        }

        if (baseHourlyRate < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(baseHourlyRate), "Hall base hourly rate cannot be negative.");
        }

        Id = id;
        Name = name.Trim();
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
        ServiceOfferings = _serviceOfferings.AsReadOnly();
    }

    public void AddServiceOffering(ServiceOffering serviceOffering)
    {
        ArgumentNullException.ThrowIfNull(serviceOffering);

        _serviceOfferings.Add(serviceOffering);
    }
}
