using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConferenceRooms.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HallName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AttendeeCount = table.Column<int>(type: "int", nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.CheckConstraint("CK_Bookings_AttendeeCount_Positive", "[AttendeeCount] > 0");
                    table.CheckConstraint("CK_Bookings_EndAt_After_StartAt", "[EndAt] > [StartAt]");
                    table.CheckConstraint("CK_Bookings_TotalPrice_NonNegative", "[TotalPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_Bookings_Halls_HallId",
                        column: x => x.HallId,
                        principalTable: "Halls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BookedServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceServiceOfferingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookedServices", x => x.Id);
                    table.CheckConstraint("CK_BookedServices_Price_NonNegative", "[Price] >= 0");
                    table.ForeignKey(
                        name: "FK_BookedServices_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UX_BookedServices_BookingId_SourceServiceOfferingId",
                table: "BookedServices",
                columns: new[] { "BookingId", "SourceServiceOfferingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_HallId_StartAt_EndAt",
                table: "Bookings",
                columns: new[] { "HallId", "StartAt", "EndAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookedServices");

            migrationBuilder.DropTable(
                name: "Bookings");
        }
    }
}
