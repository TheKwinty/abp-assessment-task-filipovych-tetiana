using System.Net;
using System.Text.Json;

namespace ConferenceRooms.ApiTests;

public sealed class BookingReportValidationTests
    : IClassFixture<ConferenceRoomsApiFactory>
{
    private readonly ConferenceRoomsApiFactory _factory;

    public BookingReportValidationTests(ConferenceRoomsApiFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData(
        "/api/reports/bookings-summary?to=2030-10-02T06:00:00%2B03:00",
        "From")]
    [InlineData(
        "/api/reports/bookings-summary?from=2030-10-01T06:00:00%2B03:00",
        "To")]
    [InlineData(
        "/api/reports/bookings-summary?from=2030-10-01T06:00:00%2B03:00&to=2030-10-01T06:00:00%2B03:00",
        "From")]
    [InlineData(
        "/api/reports/bookings-summary?from=2030-10-02T06:00:00%2B03:00&to=2030-10-01T06:00:00%2B03:00",
        "From")]
    public async Task InvalidQuery_ReturnsValidationProblemDetails(
        string requestPath,
        string expectedErrorMember)
    {
        using var client = _factory.CreateHttpsClient();

        using var response = await client.GetAsync(requestPath);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
        Assert.Equal(400, problem.RootElement.GetProperty("status").GetInt32());
        Assert.True(problem.RootElement
            .GetProperty("errors")
            .TryGetProperty(expectedErrorMember, out _));
        Assert.False(string.IsNullOrWhiteSpace(
            problem.RootElement.GetProperty("traceId").GetString()));
    }
}
