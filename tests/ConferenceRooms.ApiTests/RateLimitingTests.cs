using System.Net;
using System.Text.Json;

namespace ConferenceRooms.ApiTests;

public sealed class RateLimitingTests
{
    [Fact]
    public async Task ThirdRequest_WhenPermitLimitIsTwo_ReturnsTooManyRequestsProblemDetails()
    {
        using var factory = new ConferenceRoomsApiFactory(
            new Dictionary<string, string?>
            {
                ["RateLimiting:PermitLimit"] = "2",
                ["RateLimiting:WindowSeconds"] = "60",
            });
        using var client = factory.CreateHttpsClient();

        using var firstResponse = await client.GetAsync("/some-route-that-does-not-exist");
        using var secondResponse = await client.GetAsync("/some-route-that-does-not-exist");
        using var rejectedResponse = await client.GetAsync("/some-route-that-does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, secondResponse.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);
        Assert.Equal(
            "application/problem+json",
            rejectedResponse.Content.Headers.ContentType?.MediaType);
        using var problem = JsonDocument.Parse(
            await rejectedResponse.Content.ReadAsStringAsync());
        Assert.Equal(429, problem.RootElement.GetProperty("status").GetInt32());
    }
}
