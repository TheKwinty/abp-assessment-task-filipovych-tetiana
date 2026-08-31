namespace ConferenceRooms.Api.Contracts.Reports;

public sealed record HallBookingSummaryResponse(
    Guid HallId,
    string HallName,
    int BookingsCount,
    decimal Revenue);
