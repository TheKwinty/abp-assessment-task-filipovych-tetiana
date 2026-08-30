using ConferenceRooms.Api.Contracts.Bookings;
using ConferenceRooms.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceRooms.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly BookingService _bookingService;

    public BookingsController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingService.GetByIdAsync(id, cancellationToken);

        return booking is null
            ? NotFound()
            : Ok(BookingResponse.FromBooking(booking));
    }

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _bookingService.CreateAsync(request, cancellationToken);

        if (result.Booking is not null)
        {
            var response = BookingResponse.FromBooking(result.Booking);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Booking.Id },
                response);
        }

        return result.Failure switch
        {
            BookingCreationFailure.HallNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Hall not found.",
                detail: "The selected Hall does not exist."),
            BookingCreationFailure.CapacityExceeded => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid attendee count.",
                detail: "Attendee count must be positive and cannot exceed Hall capacity."),
            BookingCreationFailure.InvalidTime => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid booking time.",
                detail: "Booking must be future, aligned to a 30-minute boundary, and within 06:00–23:00 on one day."),
            BookingCreationFailure.InvalidServiceSelection => Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid service selection.",
                detail: "Service IDs must be unique and belong to the selected Hall."),
            BookingCreationFailure.TimeConflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Booking time conflict.",
                detail: "The selected Hall is already booked during this interval."),
            _ => throw new InvalidOperationException(
                $"Unsupported booking creation failure: {result.Failure}."),
        };
    }
}
