using ConferenceRooms.Core.Entities;

namespace ConferenceRooms.Api.Services;

public sealed class BookingCreationResult
{
    private BookingCreationResult(
        Booking? booking,
        BookingCreationFailure? failure)
    {
        Booking = booking;
        Failure = failure;
    }

    public Booking? Booking { get; }

    public BookingCreationFailure? Failure { get; }

    public static BookingCreationResult Succeeded(Booking booking)
    {
        ArgumentNullException.ThrowIfNull(booking);

        return new BookingCreationResult(booking, null);
    }

    public static BookingCreationResult Failed(BookingCreationFailure failure)
    {
        return new BookingCreationResult(null, failure);
    }
}
