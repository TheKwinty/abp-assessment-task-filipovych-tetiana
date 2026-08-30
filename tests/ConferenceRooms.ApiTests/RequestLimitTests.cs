using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.ApiTests;

public sealed class RequestLimitTests : IClassFixture<ConferenceRoomsApiFactory>
{
    private readonly ConferenceRoomsApiFactory _factory;

    public RequestLimitTests(ConferenceRoomsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task KestrelPayloadTooLargeFailure_PreservesStatusCode()
    {
        using var client = _factory.CreateHttpsClient();

        using var response = await client.GetAsync("/__test/request-body-too-large");

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        Assert.Equal(
            "application/problem+json",
            response.Content.Headers.ContentType?.MediaType);
    }
}

[ApiController]
[Route("__test/request-body-too-large")]
public sealed class RequestBodyTooLargeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        throw new BadHttpRequestException(
            "The test request exceeds the configured body limit.",
            StatusCodes.Status413PayloadTooLarge);
    }
}
