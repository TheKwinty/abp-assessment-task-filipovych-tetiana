using ConferenceRooms.Api.Contracts.Halls;
using ConferenceRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Api.Controllers;

[ApiController]
[Route("api/halls")]
public sealed class HallsController : ControllerBase
{
    private readonly HallService _hallService;
    private readonly HallAvailabilityService _hallAvailabilityService;

    public HallsController(
        HallService hallService,
        HallAvailabilityService hallAvailabilityService)
    {
        _hallService = hallService;
        _hallAvailabilityService = hallAvailabilityService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<HallResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var halls = await _hallService.GetAllAsync(cancellationToken);
        var response = halls
            .Select(HallResponse.FromHall)
            .ToList()
            .AsReadOnly();

        return Ok(response);
    }

    [HttpGet("available")]
    public async Task<ActionResult<IReadOnlyList<HallResponse>>> GetAvailable(
        [FromQuery] AvailabilitySearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _hallAvailabilityService.SearchAsync(
            request,
            cancellationToken);

        if (result.Halls is not null)
        {
            var response = result.Halls
                .Select(HallResponse.FromHall)
                .ToList()
                .AsReadOnly();

            return Ok(response);
        }

        return result.Failure switch
        {
            HallAvailabilityFailure.InvalidTime => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid availability search time.",
                detail: "Search must be future, full-hour aligned, and within 06:00–23:00 on one day."),
            _ => throw new InvalidOperationException(
                $"Unsupported Hall availability failure: {result.Failure}."),
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HallResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var hall = await _hallService.GetByIdAsync(id, cancellationToken);

        return hall is null
            ? NotFound()
            : Ok(HallResponse.FromHall(hall));
    }

    [HttpPost]
    public async Task<ActionResult<HallResponse>> Create(
        CreateHallRequest request,
        CancellationToken cancellationToken)
    {
        var hall = await _hallService.CreateAsync(request, cancellationToken);
        var response = HallResponse.FromHall(hall);

        return CreatedAtAction(nameof(GetById), new { id = hall.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateHallRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _hallService.UpdateAsync(id, request, cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _hallService.DeleteAsync(id, cancellationToken);

        return result switch
        {
            HallDeletionResult.Deleted => NoContent(),
            HallDeletionResult.NotFound => NotFound(),
            HallDeletionResult.HasBookings => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Hall cannot be deleted.",
                detail: "Historical or current bookings reference this Hall."),
            _ => throw new InvalidOperationException(
                $"Unsupported Hall deletion result: {result}."),
        };
    }
}
