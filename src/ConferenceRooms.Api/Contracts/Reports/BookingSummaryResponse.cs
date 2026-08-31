namespace ConferenceRooms.Api.Contracts.Reports;

public sealed record BookingSummaryResponse(
    DateTimeOffset From,
    DateTimeOffset To,
    int TotalBookings,
    decimal TotalRevenue,
    IReadOnlyList<HallBookingSummaryResponse> Halls);
