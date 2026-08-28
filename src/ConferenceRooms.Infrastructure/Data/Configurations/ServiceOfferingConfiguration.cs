using ConferenceRooms.Core.Entities;
using ConferenceRooms.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRooms.Infrastructure.Data.Configurations;

internal sealed class ServiceOfferingConfiguration : IEntityTypeConfiguration<ServiceOffering>
{
    public void Configure(EntityTypeBuilder<ServiceOffering> builder)
    {
        builder.ToTable("ServiceOfferings");

        builder.HasKey(serviceOffering => serviceOffering.Id);

        builder
            .Property(serviceOffering => serviceOffering.Id)
            .ValueGeneratedNever();

        builder
            .Property(serviceOffering => serviceOffering.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(serviceOffering => serviceOffering.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder
            .Property<Guid>("HallId")
            .IsRequired();

        builder
            .HasIndex("HallId")
            .HasDatabaseName("IX_ServiceOfferings_HallId");

        DatabaseSeedData.ConfigureServiceOfferings(builder);
    }
}
