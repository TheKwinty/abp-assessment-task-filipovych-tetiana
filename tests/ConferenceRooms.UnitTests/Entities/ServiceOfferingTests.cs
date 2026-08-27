using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.UnitTests.Entities;

public sealed class ServiceOfferingTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesServiceOffering()
    {
        var id = Guid.NewGuid();

        var serviceOffering = new ServiceOffering(id, "  Projector  ", 400m);

        Assert.Equal(id, serviceOffering.Id);
        Assert.Equal("Projector", serviceOffering.Name);
        Assert.Equal(400m, serviceOffering.Price);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new ServiceOffering(Guid.Empty, "Projector", 400m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(
            () => new ServiceOffering(Guid.NewGuid(), name!, 400m));
    }

    [Fact]
    public void Constructor_WithZeroPrice_CreatesServiceOffering()
    {
        var serviceOffering = new ServiceOffering(Guid.NewGuid(), "Wi-Fi", 0m);

        Assert.Equal(0m, serviceOffering.Price);
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ServiceOffering(Guid.NewGuid(), "Projector", -0.01m));
    }
}
