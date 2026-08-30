namespace ConferenceRooms.ApiTests;

public sealed class ConfigurationValidationTests
{
    [Theory]
    [InlineData("RateLimiting:PermitLimit", "0")]
    [InlineData("RateLimiting:PermitLimit", "-1")]
    [InlineData("RateLimiting:WindowSeconds", "0")]
    [InlineData("RateLimiting:WindowSeconds", "-1")]
    [InlineData("RequestLimits:MaxRequestBodySizeBytes", "0")]
    [InlineData("RequestLimits:MaxRequestBodySizeBytes", "-1")]
    public void NonPositiveHardeningValue_FailsStartup(
        string configurationKey,
        string configurationValue)
    {
        using var factory = new ConferenceRoomsApiFactory(
            new Dictionary<string, string?>
            {
                [configurationKey] = configurationValue,
            });

        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            using var client = factory.CreateHttpsClient();
        });

        Assert.Contains(configurationKey, exception.Message, StringComparison.Ordinal);
    }
}
