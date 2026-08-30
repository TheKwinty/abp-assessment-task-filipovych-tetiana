using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ConferenceRooms.ApiTests;

public sealed class ProblemDetailsTests : IClassFixture<ConferenceRoomsApiFactory>
{
    private const string ProblemJsonMediaType = "application/problem+json";

    private readonly ConferenceRoomsApiFactory _factory;

    public ProblemDetailsTests(ConferenceRoomsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UnknownRoute_ReturnsProblemDetailsWithTraceId()
    {
        using var client = _factory.CreateHttpsClient();

        using var response = await client.GetAsync("/some-route-that-does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        using var problem = await AssertProblemDetailsAsync(response, 404);
        Assert.False(string.IsNullOrWhiteSpace(
            problem.RootElement.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task StructurallyInvalidHall_ReturnsValidationProblemDetails()
    {
        using var client = _factory.CreateHttpsClient();
        var request = new
        {
            name = "   ",
            capacity = 0,
            baseHourlyRate = 10.123m,
            services = (object?)null,
        };

        using var response = await client.PostAsJsonAsync("/api/halls", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var problem = await AssertProblemDetailsAsync(response, 400);
        var errors = problem.RootElement.GetProperty("errors");
        Assert.Equal(JsonValueKind.Object, errors.ValueKind);
        Assert.NotEmpty(errors.EnumerateObject());
    }

    [Fact]
    public async Task UnhandledDatabaseFailure_ReturnsSafeProblemDetails()
    {
        using var client = _factory.CreateHttpsClient();

        using var response = await client.GetAsync("/api/halls");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var problem = await AssertProblemDetailsAsync(response, 500, body);
        Assert.False(string.IsNullOrWhiteSpace(
            problem.RootElement.GetProperty("traceId").GetString()));
        Assert.DoesNotContain("SqlException", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Server=", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(".cs:line", body, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<JsonDocument> AssertProblemDetailsAsync(
        HttpResponseMessage response,
        int expectedStatus,
        string? body = null)
    {
        Assert.Equal(ProblemJsonMediaType, response.Content.Headers.ContentType?.MediaType);
        body ??= await response.Content.ReadAsStringAsync();
        var problem = JsonDocument.Parse(body);
        Assert.Equal(expectedStatus, problem.RootElement.GetProperty("status").GetInt32());

        return problem;
    }
}
