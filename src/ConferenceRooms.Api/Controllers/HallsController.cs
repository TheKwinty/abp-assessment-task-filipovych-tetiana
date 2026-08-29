using ConferenceRooms.Api.Contracts.Halls;
using ConferenceRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Api.Controllers;

[ApiController]
[Route("api/halls")]
public sealed class HallsController : ControllerBase
{
    private readonly HallService _hallService;

    public HallsController(HallService hallService)
    {
        _hallService = hallService;
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
