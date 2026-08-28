using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ConferenceRooms.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Halls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    BaseHourlyRate = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Halls", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceOfferings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    HallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceOfferings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceOfferings_Halls_HallId",
                        column: x => x.HallId,
                        principalTable: "Halls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Halls",
                columns: new[] { "Id", "BaseHourlyRate", "Capacity", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 2000m, 50, "Hall A" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 3500m, 100, "Hall B" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 1500m, 30, "Hall C" }
                });

            migrationBuilder.InsertData(
                table: "ServiceOfferings",
                columns: new[] { "Id", "HallId", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), "Projector", 500m },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), "Wi-Fi", 300m },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), "Sound", 700m },
                    { new Guid("b0000000-0000-0000-0000-000000000001"), new Guid("22222222-2222-2222-2222-222222222222"), "Projector", 500m },
                    { new Guid("b0000000-0000-0000-0000-000000000002"), new Guid("22222222-2222-2222-2222-222222222222"), "Wi-Fi", 300m },
                    { new Guid("b0000000-0000-0000-0000-000000000003"), new Guid("22222222-2222-2222-2222-222222222222"), "Sound", 700m },
                    { new Guid("c0000000-0000-0000-0000-000000000001"), new Guid("33333333-3333-3333-3333-333333333333"), "Projector", 500m },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), new Guid("33333333-3333-3333-3333-333333333333"), "Wi-Fi", 300m },
                    { new Guid("c0000000-0000-0000-0000-000000000003"), new Guid("33333333-3333-3333-3333-333333333333"), "Sound", 700m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceOfferings_HallId",
                table: "ServiceOfferings",
                column: "HallId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceOfferings");

            migrationBuilder.DropTable(
                name: "Halls");
        }
    }
}
