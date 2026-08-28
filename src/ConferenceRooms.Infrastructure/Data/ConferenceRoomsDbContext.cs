using ConferenceRooms.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConferenceRooms.Infrastructure.Data;

public sealed class ConferenceRoomsDbContext : DbContext
{
    public ConferenceRoomsDbContext(
        DbContextOptions<ConferenceRoomsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hall> Halls => Set<Hall>();

    public DbSet<ServiceOffering> ServiceOfferings => Set<ServiceOffering>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ConferenceRoomsDbContext).Assembly);
    }
}
