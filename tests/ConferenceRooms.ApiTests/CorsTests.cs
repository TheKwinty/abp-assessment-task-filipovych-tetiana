namespace ConferenceRooms.ApiTests;

public sealed class CorsTests
{
    private const string AllowedOrigin = "https://client.example";

    [Fact]
    public async Task Preflight_FromAllowedOrigin_ReturnsExactOriginHeader()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateHttpsClient();
        using var request = CreatePreflightRequest(AllowedOrigin);

        using var response = await client.SendAsync(request);

        var allowedOrigins = response.Headers.GetValues("Access-Control-Allow-Origin");
        Assert.Equal([AllowedOrigin], allowedOrigins);
    }

    [Fact]
    public async Task Preflight_FromRejectedOrigin_DoesNotReturnAllowOriginHeader()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateHttpsClient();
        using var request = CreatePreflightRequest("https://evil.example");

        using var response = await client.SendAsync(request);

        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static ConferenceRoomsApiFactory CreateFactory()
    {
        return new ConferenceRoomsApiFactory(new Dictionary<string, string?>
        {
            ["Cors:AllowedOrigins:0"] = AllowedOrigin,
        });
    }

    private static HttpRequestMessage CreatePreflightRequest(string origin)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, "/api/halls");
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");

        return request;
    }
}
