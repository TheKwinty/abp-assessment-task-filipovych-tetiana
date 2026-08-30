using ConferenceRooms.Api.Contracts.Halls;
using ConferenceRooms.Core.Scheduling;
using ConferenceRooms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Api.Services;

public sealed class HallAvailabilityService
{
    private readonly ConferenceRoomsDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public HallAvailabilityService(
        ConferenceRoomsDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<HallAvailabilityResult> SearchAsync(
        AvailabilitySearchRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var start = request.Start!.Value;
        var durationHours = request.DurationHours!.Value;

        if (start <= _timeProvider.GetUtcNow()
            || !BookingTimeRules.TryCreateWindow(start, durationHours, out var end))
        {
            return HallAvailabilityResult.Failed(HallAvailabilityFailure.InvalidTime);
        }

        var capacity = request.Capacity!.Value;
        var halls = await _dbContext.Halls
            .AsNoTracking()
            .Where(hall => hall.Capacity >= capacity)
            .Where(hall => !_dbContext.Bookings.Any(booking =>
                booking.HallId == hall.Id
                && booking.StartAt < end
                && start < booking.EndAt))
            .Include(hall => hall.ServiceOfferings)
            .OrderBy(hall => hall.Capacity)
            .ThenBy(hall => hall.Name)
            .ThenBy(hall => hall.Id)
            .ToListAsync(cancellationToken);

        return HallAvailabilityResult.Succeeded(halls.AsReadOnly());
    }
}
