using ConferenceRooms.Api.Contracts.Reports;
using ConferenceRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Api.Controllers;

[ApiController]
[Route("api/reports")]
public sealed class ReportsController : ControllerBase
{
    private readonly BookingReportService _bookingReportService;

    public ReportsController(BookingReportService bookingReportService)
    {
        _bookingReportService = bookingReportService;
    }

    [HttpGet("bookings-summary")]
    public async Task<ActionResult<BookingSummaryResponse>> GetBookingSummary(
        [FromQuery] BookingSummaryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _bookingReportService.GetSummaryAsync(
            request.From!.Value,
            request.To!.Value,
            cancellationToken);

        return Ok(response);
    }
}
