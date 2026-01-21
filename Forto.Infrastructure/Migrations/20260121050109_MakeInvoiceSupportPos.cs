using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeInvoiceSupportPos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Bookings_BookingId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BookingId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.AlterColumn<int>(
                name: "BookingId",
                schema: "billing",
                table: "Invoices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                schema: "billing",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BookingId",
                schema: "billing",
                table: "Invoices",
                column: "BookingId",
                unique: true,
                filter: "[BookingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BranchId",
                schema: "billing",
                table: "Invoices",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Bookings_BookingId",
                schema: "billing",
                table: "Invoices",
                column: "BookingId",
                principalSchema: "booking",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Branches_BranchId",
                schema: "billing",
                table: "Invoices",
                column: "BranchId",
                principalSchema: "ops",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Bookings_BookingId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Branches_BranchId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BookingId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_BranchId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.AlterColumn<int>(
                name: "BookingId",
                schema: "billing",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BookingId",
                schema: "billing",
                table: "Invoices",
                column: "BookingId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Bookings_BookingId",
                schema: "billing",
                table: "Invoices",
                column: "BookingId",
                principalSchema: "booking",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
