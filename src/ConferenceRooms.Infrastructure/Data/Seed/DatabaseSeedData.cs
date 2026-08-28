using ConferenceRooms.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConferenceRooms.Infrastructure.Data.Seed;

internal static class DatabaseSeedData
{
    private static readonly Guid HallAId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid HallBId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid HallCId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    internal static void ConfigureHalls(EntityTypeBuilder<Hall> builder)
    {
        builder.HasData(
            new
            {
                Id = HallAId,
                Name = "Hall A",
                Capacity = 50,
                BaseHourlyRate = 2000m,
            },
            new
            {
                Id = HallBId,
                Name = "Hall B",
                Capacity = 100,
                BaseHourlyRate = 3500m,
            },
            new
            {
                Id = HallCId,
                Name = "Hall C",
                Capacity = 30,
                BaseHourlyRate = 1500m,
            });
    }

    internal static void ConfigureServiceOfferings(
        EntityTypeBuilder<ServiceOffering> builder)
    {
        builder.HasData(
            CreateServiceOffering(
                "a0000000-0000-0000-0000-000000000001",
                "Projector",
                500m,
                HallAId),
            CreateServiceOffering(
                "a0000000-0000-0000-0000-000000000002",
                "Wi-Fi",
                300m,
                HallAId),
            CreateServiceOffering(
                "a0000000-0000-0000-0000-000000000003",
                "Sound",
                700m,
                HallAId),
            CreateServiceOffering(
                "b0000000-0000-0000-0000-000000000001",
                "Projector",
                500m,
                HallBId),
            CreateServiceOffering(
                "b0000000-0000-0000-0000-000000000002",
                "Wi-Fi",
                300m,
                HallBId),
            CreateServiceOffering(
                "b0000000-0000-0000-0000-000000000003",
                "Sound",
                700m,
                HallBId),
            CreateServiceOffering(
                "c0000000-0000-0000-0000-000000000001",
                "Projector",
                500m,
                HallCId),
            CreateServiceOffering(
                "c0000000-0000-0000-0000-000000000002",
                "Wi-Fi",
                300m,
                HallCId),
            CreateServiceOffering(
                "c0000000-0000-0000-0000-000000000003",
                "Sound",
                700m,
                HallCId));
    }

    private static object CreateServiceOffering(
        string id,
        string name,
        decimal price,
        Guid hallId)
    {
        return new
        {
            Id = Guid.Parse(id),
            Name = name,
            Price = price,
            HallId = hallId,
        };
    }
}
