using ConferenceRooms.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRooms.Infrastructure.Data.Configurations;

internal sealed class BookedServiceConfiguration : IEntityTypeConfiguration<BookedService>
{
    public void Configure(EntityTypeBuilder<BookedService> builder)
    {
        builder.ToTable(
            "BookedServices",
            tableBuilder => tableBuilder.HasCheckConstraint(
                "CK_BookedServices_Price_NonNegative",
                "[Price] >= 0"));

        builder.HasKey(bookedService => bookedService.Id);

        builder
            .Property(bookedService => bookedService.Id)
            .ValueGeneratedNever();

        builder
            .Property<Guid>("BookingId")
            .IsRequired();

        builder
            .Property(bookedService => bookedService.SourceServiceOfferingId)
            .IsRequired();

        builder
            .Property(bookedService => bookedService.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(bookedService => bookedService.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder
            .HasIndex("BookingId", nameof(BookedService.SourceServiceOfferingId))
            .IsUnique()
            .HasDatabaseName("UX_BookedServices_BookingId_SourceServiceOfferingId");
    }
}
