using ConferenceRooms.Core.Entities;
using ConferenceRooms.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRooms.Infrastructure.Data.Configurations;

internal sealed class HallConfiguration : IEntityTypeConfiguration<Hall>
{
    public void Configure(EntityTypeBuilder<Hall> builder)
    {
        builder.ToTable("Halls");

        builder.HasKey(hall => hall.Id);

        builder
            .Property(hall => hall.Id)
            .ValueGeneratedNever();

        builder
            .Property(hall => hall.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(hall => hall.Capacity)
            .IsRequired();

        builder
            .Property(hall => hall.BaseHourlyRate)
            .IsRequired()
            .HasPrecision(18, 2);

        builder
            .HasMany(hall => hall.ServiceOfferings)
            .WithOne()
            .HasForeignKey("HallId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(hall => hall.ServiceOfferings)
            .HasField("_serviceOfferings")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        DatabaseSeedData.ConfigureHalls(builder);
    }
}
