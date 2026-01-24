using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialChangeRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingItemMaterialChangeRequests",
                schema: "ops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingItemId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedByCashierId = table.Column<int>(type: "int", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItemMaterialChangeRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookingItemMaterialChangeRequestLines",
                schema: "ops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    ProposedActualQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItemMaterialChangeRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingItemMaterialChangeRequestLines_BookingItemMaterialChangeRequests_RequestId",
                        column: x => x.RequestId,
                        principalSchema: "ops",
                        principalTable: "BookingItemMaterialChangeRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingItemMaterialChangeRequestLines_RequestId",
                schema: "ops",
                table: "BookingItemMaterialChangeRequestLines",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingItemMaterialChangeRequests_BookingItemId_Status",
                schema: "ops",
                table: "BookingItemMaterialChangeRequests",
                columns: new[] { "BookingItemId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingItemMaterialChangeRequestLines",
                schema: "ops");

            migrationBuilder.DropTable(
                name: "BookingItemMaterialChangeRequests",
                schema: "ops");
        }
    }
}
