namespace ConferenceRooms.Api.Services;

public enum HallDeletionResult
{
    Deleted,
    NotFound,
    HasBookings,
}
