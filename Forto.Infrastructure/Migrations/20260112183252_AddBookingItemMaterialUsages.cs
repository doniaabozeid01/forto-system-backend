using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingItemMaterialUsages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookingItemMaterialUsages",
                schema: "ops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingItemId = table.Column<int>(type: "int", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    DefaultQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReservedQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ActualQty = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitCharge = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ExtraCharge = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RecordedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookingItemMaterialUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingItemMaterialUsages_BookingItems_BookingItemId",
                        column: x => x.BookingItemId,
                        principalSchema: "booking",
                        principalTable: "BookingItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BookingItemMaterialUsages_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalSchema: "inventory",
                        principalTable: "Materials",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingItemMaterialUsages_BookingItemId_MaterialId",
                schema: "ops",
                table: "BookingItemMaterialUsages",
                columns: new[] { "BookingItemId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingItemMaterialUsages_MaterialId",
                schema: "ops",
                table: "BookingItemMaterialUsages",
                column: "MaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingItemMaterialUsages",
                schema: "ops");
        }
    }
}
