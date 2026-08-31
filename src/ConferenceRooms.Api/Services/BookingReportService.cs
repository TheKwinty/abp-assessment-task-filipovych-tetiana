using ConferenceRooms.Api.Contracts.Reports;
using ConferenceRooms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Api.Services;

public sealed class BookingReportService
{
    private readonly ConferenceRoomsDbContext _dbContext;

    public BookingReportService(ConferenceRoomsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookingSummaryResponse> GetSummaryAsync(
        DateTimeOffset periodStart,
        DateTimeOffset periodEnd,
        CancellationToken cancellationToken = default)
    {
        var hallSummaries = await (
            from booking in _dbContext.Bookings.AsNoTracking()
            join hall in _dbContext.Halls.AsNoTracking()
                on booking.HallId equals hall.Id
            where booking.StartAt >= periodStart && booking.StartAt < periodEnd
            group booking by new
            {
                HallId = hall.Id,
                HallName = hall.Name,
            }
            into bookingsByHall
            orderby bookingsByHall.Key.HallName, bookingsByHall.Key.HallId
            select new HallBookingSummaryResponse(
                bookingsByHall.Key.HallId,
                bookingsByHall.Key.HallName,
                bookingsByHall.Count(),
                bookingsByHall.Sum(booking => booking.TotalPrice)))
            .ToListAsync(cancellationToken);

        var readOnlyHallSummaries = hallSummaries.AsReadOnly();

        return new BookingSummaryResponse(
            periodStart,
            periodEnd,
            readOnlyHallSummaries.Sum(summary => summary.BookingsCount),
            readOnlyHallSummaries.Sum(summary => summary.Revenue),
            readOnlyHallSummaries);
    }
}
