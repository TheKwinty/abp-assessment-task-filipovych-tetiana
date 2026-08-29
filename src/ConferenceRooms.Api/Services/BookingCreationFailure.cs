namespace ConferenceRooms.Api.Services;

public enum BookingCreationFailure
{
    HallNotFound,
    CapacityExceeded,
    InvalidTime,
    InvalidServiceSelection,
    TimeConflict,
}
