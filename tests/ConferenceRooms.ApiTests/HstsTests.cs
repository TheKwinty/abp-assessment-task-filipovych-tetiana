namespace ConferenceRooms.ApiTests;

public sealed class HstsTests
{
    [Fact]
    public async Task HttpsResponse_InProduction_IncludesStrictTransportSecurityHeader()
    {
        using var factory = new ConferenceRoomsApiFactory(
            new Dictionary<string, string?>(),
            "Production");
        using var client = factory.CreateHttpsClient();
        client.BaseAddress = new Uri("https://conference-rooms.test");

        using var response = await client.GetAsync("/some-route-that-does-not-exist");

        Assert.True(response.Headers.Contains("Strict-Transport-Security"));
    }
}
