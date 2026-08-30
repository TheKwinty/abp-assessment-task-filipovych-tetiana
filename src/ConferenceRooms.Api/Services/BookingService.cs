using System.Data;
using ConferenceRooms.Api.Contracts.Bookings;
using ConferenceRooms.Core.Entities;
using ConferenceRooms.Core.Pricing;
using ConferenceRooms.Core.Scheduling;
using ConferenceRooms.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Api.Services;

public sealed class BookingService
{
    private const int MaximumAttempts = 3;

    private const int DeadlockVictimErrorNumber = 1205;

    private readonly ConferenceRoomsDbContext _dbContext;
    private readonly RentalPriceCalculator _rentalPriceCalculator;
    private readonly TimeProvider _timeProvider;

    public BookingService(
        ConferenceRoomsDbContext dbContext,
        RentalPriceCalculator rentalPriceCalculator,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _rentalPriceCalculator = rentalPriceCalculator;
        _timeProvider = timeProvider;
    }

    public async Task<Booking?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.BookedServices)
            .SingleOrDefaultAsync(booking => booking.Id == id, cancellationToken);
    }

    public async Task<BookingCreationResult> CreateAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        for (var attempt = 1; attempt <= MaximumAttempts; attempt++)
        {
            try
            {
                return await CreateAttemptAsync(request, cancellationToken);
            }
            catch (SqlException exception)
                when (attempt < MaximumAttempts && IsRetryable(exception))
            {
                await PrepareForRetryAsync(attempt, cancellationToken);
            }
            catch (DbUpdateException exception)
                when (attempt < MaximumAttempts
                    && ContainsRetryableSqlException(exception))
            {
                await PrepareForRetryAsync(attempt, cancellationToken);
            }
        }

        throw new InvalidOperationException("The bounded booking retry loop terminated unexpectedly.");
    }

    private async Task<BookingCreationResult> CreateAttemptAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var hall = await _dbContext.Halls
            .Include(existingHall => existingHall.ServiceOfferings)
            .SingleOrDefaultAsync(
                existingHall => existingHall.Id == request.HallId,
                cancellationToken);

        if (hall is null)
        {
            return BookingCreationResult.Failed(BookingCreationFailure.HallNotFound);
        }

        if (!TryCreateTimeWindow(request, out var start, out var duration, out var end))
        {
            return BookingCreationResult.Failed(BookingCreationFailure.InvalidTime);
        }

        if (request.AttendeeCount is not int attendeeCount
            || attendeeCount <= 0
            || attendeeCount > hall.Capacity)
        {
            return BookingCreationResult.Failed(BookingCreationFailure.CapacityExceeded);
        }

        if (!TrySelectServices(
                hall,
                request.ServiceOfferingIds,
                out var selectedServiceIds,
                out var selectedServices))
        {
            return BookingCreationResult.Failed(
                BookingCreationFailure.InvalidServiceSelection);
        }

        var overlapsExistingBooking = await _dbContext.Bookings.AnyAsync(
            existingBooking =>
                existingBooking.HallId == hall.Id
                && existingBooking.StartAt < end
                && start < existingBooking.EndAt,
            cancellationToken);

        if (overlapsExistingBooking)
        {
            return BookingCreationResult.Failed(BookingCreationFailure.TimeConflict);
        }

        var totalPrice = _rentalPriceCalculator.Calculate(
            hall,
            start,
            duration,
            selectedServiceIds);

        var serviceSnapshots = selectedServices
            .Select(serviceOffering => new BookedService(
                Guid.NewGuid(),
                serviceOffering.Id,
                serviceOffering.Name,
                serviceOffering.Price))
            .ToList();

        var booking = new Booking(
            Guid.NewGuid(),
            hall.Id,
            hall.Name,
            attendeeCount,
            start,
            end,
            totalPrice,
            _timeProvider.GetUtcNow(),
            serviceSnapshots);

        _dbContext.Bookings.Add(booking);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return BookingCreationResult.Succeeded(booking);
    }

    private bool TryCreateTimeWindow(
        CreateBookingRequest request,
        out DateTimeOffset start,
        out TimeSpan duration,
        out DateTimeOffset end)
    {
        start = request.Start ?? default;
        duration = default;
        end = default;

        if (request.Start is null
            || request.DurationHours is not int durationHours
            || start <= _timeProvider.GetUtcNow()
            || !BookingTimeRules.TryCreateWindow(start, durationHours, out end))
        {
            return false;
        }

        duration = TimeSpan.FromHours(durationHours);
        return true;
    }

    private static bool TrySelectServices(
        Hall hall,
        IReadOnlyList<Guid>? requestedServiceIds,
        out IReadOnlyList<Guid> selectedServiceIds,
        out IReadOnlyList<ServiceOffering> selectedServices)
    {
        selectedServiceIds = [];
        selectedServices = [];

        if (requestedServiceIds is null)
        {
            return false;
        }

        var hallServicesById = hall.ServiceOfferings.ToDictionary(
            serviceOffering => serviceOffering.Id);
        var uniqueServiceIds = new HashSet<Guid>();
        var serviceIds = new List<Guid>(requestedServiceIds.Count);
        var services = new List<ServiceOffering>(requestedServiceIds.Count);

        foreach (var serviceOfferingId in requestedServiceIds)
        {
            if (!uniqueServiceIds.Add(serviceOfferingId)
                || !hallServicesById.TryGetValue(serviceOfferingId, out var serviceOffering))
            {
                return false;
            }

            serviceIds.Add(serviceOfferingId);
            services.Add(serviceOffering);
        }

        selectedServiceIds = serviceIds.AsReadOnly();
        selectedServices = services.AsReadOnly();

        return true;
    }

    private async Task PrepareForRetryAsync(
        int completedAttempt,
        CancellationToken cancellationToken)
    {
        _dbContext.ChangeTracker.Clear();

        await Task.Delay(
            TimeSpan.FromMilliseconds(50 * completedAttempt),
            _timeProvider,
            cancellationToken);
    }

    private static bool IsRetryable(SqlException exception)
    {
        return exception.Errors
            .Cast<SqlError>()
            .Any(error => error.Number == DeadlockVictimErrorNumber);
    }

    private static bool ContainsRetryableSqlException(Exception exception)
    {
        for (Exception? currentException = exception;
             currentException is not null;
             currentException = currentException.InnerException)
        {
            if (currentException is SqlException sqlException)
            {
                return IsRetryable(sqlException);
            }
        }

        return false;
    }
}
