using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.UnitTests.Entities;

public sealed class HallTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesHallWithEmptyServiceOfferings()
    {
        var id = Guid.NewGuid();

        var hall = new Hall(id, "  Hall A  ", 20, 1000.00m);

        Assert.Equal(id, hall.Id);
        Assert.Equal("Hall A", hall.Name);
        Assert.Equal(20, hall.Capacity);
        Assert.Equal(1000.00m, hall.BaseHourlyRate);
        Assert.Empty(hall.ServiceOfferings);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new Hall(Guid.Empty, "Hall A", 20, 1000m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(
            () => new Hall(Guid.NewGuid(), name!, 20, 1000m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WithNonPositiveCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Hall(Guid.NewGuid(), "Hall A", capacity, 1000m));
    }

    [Fact]
    public void Constructor_WithZeroBaseHourlyRate_CreatesHall()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 0m);

        Assert.Equal(0m, hall.BaseHourlyRate);
    }

    [Fact]
    public void Constructor_WithNegativeBaseHourlyRate_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Hall(Guid.NewGuid(), "Hall A", 20, -0.01m));
    }

    [Fact]
    public void ServiceOfferings_CannotBeModifiedExternally()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);
        var serviceOfferings = Assert.IsAssignableFrom<ICollection<ServiceOffering>>(
            hall.ServiceOfferings);
        var serviceOffering = new ServiceOffering(Guid.NewGuid(), "Projector", 400m);

        Assert.True(serviceOfferings.IsReadOnly);
        Assert.Throws<NotSupportedException>(() => serviceOfferings.Add(serviceOffering));
        Assert.Empty(hall.ServiceOfferings);
    }

    [Fact]
    public void AddServiceOffering_WithValidService_ExposesAddedService()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);
        var serviceOffering = new ServiceOffering(Guid.NewGuid(), "Projector", 400m);
        var serviceOfferings = hall.ServiceOfferings;

        hall.AddServiceOffering(serviceOffering);

        Assert.Same(serviceOffering, Assert.Single(serviceOfferings));
    }

    [Fact]
    public void AddServiceOffering_WithNullService_ThrowsArgumentNullException()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);

        Assert.Throws<ArgumentNullException>(() => hall.AddServiceOffering(null!));
    }

    [Fact]
    public void UpdateDetails_WithValidValues_UpdatesHall()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);

        hall.UpdateDetails("Hall B", 40, 2500m);

        Assert.Equal("Hall B", hall.Name);
        Assert.Equal(40, hall.Capacity);
        Assert.Equal(2500m, hall.BaseHourlyRate);
    }

    [Fact]
    public void UpdateDetails_TrimsName()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);

        hall.UpdateDetails("  Hall B  ", 40, 2500m);

        Assert.Equal("Hall B", hall.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);

        Assert.Throws<ArgumentException>(
            () => hall.UpdateDetails(name!, 40, 2500m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateDetails_WithNonPositiveCapacity_ThrowsArgumentOutOfRangeException(int capacity)
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => hall.UpdateDetails("Hall B", capacity, 2500m));
    }

    [Fact]
    public void UpdateDetails_WithNegativeBaseHourlyRate_ThrowsArgumentOutOfRangeException()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => hall.UpdateDetails("Hall B", 40, -0.01m));
    }

    [Fact]
    public void ReplaceServiceOfferings_WithValidCollection_ReplacesExistingServices()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);
        hall.AddServiceOffering(new ServiceOffering(Guid.NewGuid(), "Projector", 400m));
        hall.AddServiceOffering(new ServiceOffering(Guid.NewGuid(), "Wi-Fi", 200m));
        var sound = new ServiceOffering(Guid.NewGuid(), "Sound", 600m);

        hall.ReplaceServiceOfferings([sound]);

        Assert.Same(sound, Assert.Single(hall.ServiceOfferings));
    }

    [Fact]
    public void ReplaceServiceOfferings_WithEmptyCollection_RemovesAllServices()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);
        hall.AddServiceOffering(new ServiceOffering(Guid.NewGuid(), "Projector", 400m));

        hall.ReplaceServiceOfferings([]);

        Assert.Empty(hall.ServiceOfferings);
    }

    [Fact]
    public void ReplaceServiceOfferings_WithNullCollection_ThrowsArgumentNullException()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);
        var projector = new ServiceOffering(Guid.NewGuid(), "Projector", 400m);
        hall.AddServiceOffering(projector);

        Assert.Throws<ArgumentNullException>(
            () => hall.ReplaceServiceOfferings(null!));
        Assert.Same(projector, Assert.Single(hall.ServiceOfferings));
    }

    [Fact]
    public void ReplaceServiceOfferings_WithNullItem_ThrowsArgumentException()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);

        Assert.Throws<ArgumentException>(
            () => hall.ReplaceServiceOfferings([null!]));
    }

    [Fact]
    public void ReplaceServiceOfferings_WithInvalidCollection_DoesNotModifyExistingServices()
    {
        var hall = new Hall(Guid.NewGuid(), "Hall A", 20, 1000m);
        var projector = new ServiceOffering(Guid.NewGuid(), "Projector", 400m);
        var wiFi = new ServiceOffering(Guid.NewGuid(), "Wi-Fi", 200m);
        hall.AddServiceOffering(projector);
        hall.AddServiceOffering(wiFi);
        var sound = new ServiceOffering(Guid.NewGuid(), "Sound", 600m);

        Assert.Throws<ArgumentException>(
            () => hall.ReplaceServiceOfferings([sound, null!]));

        Assert.Equal([projector, wiFi], hall.ServiceOfferings);
    }
}
