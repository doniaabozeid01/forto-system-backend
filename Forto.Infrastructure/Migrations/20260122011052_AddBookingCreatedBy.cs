using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingCreatedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByClientId",
                schema: "booking",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByEmployeeId",
                schema: "booking",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByType",
                schema: "booking",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                schema: "booking",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByClientId",
                schema: "booking",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                schema: "booking",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatedByType",
                schema: "booking",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                schema: "booking",
                table: "Bookings");
        }
    }
}
