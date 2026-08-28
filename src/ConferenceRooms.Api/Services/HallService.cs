using ConferenceRooms.Api.Contracts.Halls;
using ConferenceRooms.Core.Entities;
using ConferenceRooms.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Api.Services;

public sealed class HallService
{
    private readonly ConferenceRoomsDbContext _dbContext;

    public HallService(ConferenceRoomsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Hall>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Halls
            .AsNoTracking()
            .Include(hall => hall.ServiceOfferings)
            .OrderBy(hall => hall.Name)
            .ThenBy(hall => hall.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Hall?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Halls
            .AsNoTracking()
            .Include(hall => hall.ServiceOfferings)
            .SingleOrDefaultAsync(hall => hall.Id == id, cancellationToken);
    }

    public async Task<Hall> CreateAsync(
        CreateHallRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var hall = new Hall(
            Guid.NewGuid(),
            request.Name!,
            request.Capacity!.Value,
            request.BaseHourlyRate!.Value);

        foreach (var requestedService in request.Services!)
        {
            hall.AddServiceOffering(CreateServiceOffering(requestedService));
        }

        _dbContext.Halls.Add(hall);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return hall;
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        UpdateHallRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var hall = await _dbContext.Halls
            .Include(existingHall => existingHall.ServiceOfferings)
            .SingleOrDefaultAsync(existingHall => existingHall.Id == id, cancellationToken);

        if (hall is null)
        {
            return false;
        }

        hall.UpdateDetails(
            request.Name!,
            request.Capacity!.Value,
            request.BaseHourlyRate!.Value);

        var replacementServices = request.Services!
            .Select(CreateServiceOffering)
            .ToList();

        hall.ReplaceServiceOfferings(replacementServices);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var hall = await _dbContext.Halls
            .SingleOrDefaultAsync(existingHall => existingHall.Id == id, cancellationToken);

        if (hall is null)
        {
            return false;
        }

        _dbContext.Halls.Remove(hall);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static ServiceOffering CreateServiceOffering(
        ServiceOfferingRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new ServiceOffering(
            Guid.NewGuid(),
            request.Name!,
            request.Price!.Value);
    }
}
