using ConferenceRooms.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRooms.Infrastructure.Data.Configurations;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable(
            "Bookings",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Bookings_AttendeeCount_Positive",
                    "[AttendeeCount] > 0");
                tableBuilder.HasCheckConstraint(
                    "CK_Bookings_EndAt_After_StartAt",
                    "[EndAt] > [StartAt]");
                tableBuilder.HasCheckConstraint(
                    "CK_Bookings_TotalPrice_NonNegative",
                    "[TotalPrice] >= 0");
            });

        builder.HasKey(booking => booking.Id);

        builder
            .Property(booking => booking.Id)
            .ValueGeneratedNever();

        builder
            .Property(booking => booking.HallName)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .Property(booking => booking.AttendeeCount)
            .IsRequired();

        builder
            .Property(booking => booking.StartAt)
            .IsRequired();

        builder
            .Property(booking => booking.EndAt)
            .IsRequired();

        builder
            .Property(booking => booking.TotalPrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder
            .Property(booking => booking.CreatedAt)
            .IsRequired();

        builder.Ignore(booking => booking.Duration);

        builder
            .HasOne<Hall>()
            .WithMany()
            .HasForeignKey(booking => booking.HallId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(booking => booking.BookedServices)
            .WithOne()
            .HasForeignKey("BookingId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Navigation(booking => booking.BookedServices)
            .HasField("_bookedServices")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .HasIndex(booking => new
            {
                booking.HallId,
                booking.StartAt,
                booking.EndAt,
            })
            .HasDatabaseName("IX_Bookings_HallId_StartAt_EndAt");
    }
}
