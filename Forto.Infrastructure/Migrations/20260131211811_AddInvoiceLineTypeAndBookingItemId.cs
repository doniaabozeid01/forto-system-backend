using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceLineTypeAndBookingItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_InvoiceId",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.AddColumn<int>(
                name: "BookingItemId",
                schema: "billing",
                table: "InvoiceLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LineType",
                schema: "billing",
                table: "InvoiceLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId_LineType",
                schema: "billing",
                table: "InvoiceLines",
                columns: new[] { "InvoiceId", "LineType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_InvoiceId_LineType",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "BookingItemId",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "LineType",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_InvoiceId",
                schema: "billing",
                table: "InvoiceLines",
                column: "InvoiceId");
        }
    }
}
