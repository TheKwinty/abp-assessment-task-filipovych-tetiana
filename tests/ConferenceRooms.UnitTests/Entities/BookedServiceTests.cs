using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.UnitTests.Entities;

public sealed class BookedServiceTests
{
    [Fact]
    public void Constructor_WithValidArguments_CreatesServiceSnapshot()
    {
        var id = Guid.NewGuid();
        var sourceServiceOfferingId = Guid.NewGuid();

        var bookedService = new BookedService(
            id,
            sourceServiceOfferingId,
            "  Projector  ",
            500m);

        Assert.Equal(id, bookedService.Id);
        Assert.Equal(sourceServiceOfferingId, bookedService.SourceServiceOfferingId);
        Assert.Equal("Projector", bookedService.Name);
        Assert.Equal(500m, bookedService.Price);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new BookedService(Guid.Empty, Guid.NewGuid(), "Projector", 500m));
    }

    [Fact]
    public void Constructor_WithEmptySourceServiceOfferingId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new BookedService(Guid.NewGuid(), Guid.Empty, "Projector", 500m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(
            () => new BookedService(Guid.NewGuid(), Guid.NewGuid(), name!, 500m));
    }

    [Fact]
    public void Constructor_WithZeroPrice_CreatesServiceSnapshot()
    {
        var bookedService = new BookedService(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Wi-Fi",
            0m);

        Assert.Equal(0m, bookedService.Price);
    }

    [Fact]
    public void Constructor_WithNegativePrice_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new BookedService(Guid.NewGuid(), Guid.NewGuid(), "Projector", -0.01m));
    }
}
