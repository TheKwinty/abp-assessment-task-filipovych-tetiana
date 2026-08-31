using System.Net;
using System.Text.Json;

namespace ConferenceRooms.ApiTests;

public sealed class SwaggerTests : IClassFixture<ConferenceRoomsApiFactory>
{
    private static readonly string[] ExpectedPaths =
    [
        "/api/halls",
        "/api/halls/available",
        "/api/bookings",
        "/api/reports/bookings-summary",
    ];

    private readonly ConferenceRoomsApiFactory _factory;

    public SwaggerTests(ConferenceRoomsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerJson_InDevelopment_ContainsExpectedApiPaths()
    {
        using var client = _factory.CreateHttpsClient();

        using var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        var paths = document.RootElement.GetProperty("paths");

        foreach (var expectedPath in ExpectedPaths)
        {
            Assert.True(paths.TryGetProperty(expectedPath, out _));
        }
    }

    [Fact]
    public async Task SwaggerUi_InDevelopment_ReturnsOk()
    {
        using var client = _factory.CreateHttpsClient();

        using var response = await client.GetAsync("/swagger/index.html");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
