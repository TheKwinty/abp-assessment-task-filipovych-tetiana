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

        var normalizedName = ValidateName(name);
        ValidateCapacity(capacity);
        ValidateBaseHourlyRate(baseHourlyRate);

        Id = id;
        Name = normalizedName;
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
        ServiceOfferings = _serviceOfferings.AsReadOnly();
    }

    public void AddServiceOffering(ServiceOffering serviceOffering)
    {
        ArgumentNullException.ThrowIfNull(serviceOffering);

        _serviceOfferings.Add(serviceOffering);
    }

    public void UpdateDetails(string name, int capacity, decimal baseHourlyRate)
    {
        var normalizedName = ValidateName(name);
        ValidateCapacity(capacity);
        ValidateBaseHourlyRate(baseHourlyRate);

        Name = normalizedName;
        Capacity = capacity;
        BaseHourlyRate = baseHourlyRate;
    }

    public void ReplaceServiceOfferings(IEnumerable<ServiceOffering> serviceOfferings)
    {
        ArgumentNullException.ThrowIfNull(serviceOfferings);

        var replacementServiceOfferings = serviceOfferings.ToList();

        if (replacementServiceOfferings.Any(serviceOffering => serviceOffering is null))
        {
            throw new ArgumentException(
                "Service offerings cannot contain null items.",
                nameof(serviceOfferings));
        }

        _serviceOfferings.Clear();
        _serviceOfferings.AddRange(replacementServiceOfferings);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Hall name is required.", nameof(name));
        }

        return name.Trim();
    }

    private static void ValidateCapacity(int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Hall capacity must be greater than zero.");
        }
    }

    private static void ValidateBaseHourlyRate(decimal baseHourlyRate)
    {
        if (baseHourlyRate < 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(baseHourlyRate), "Hall base hourly rate cannot be negative.");
        }
    }
}
