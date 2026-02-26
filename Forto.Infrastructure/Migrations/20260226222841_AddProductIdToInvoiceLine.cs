using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIdToInvoiceLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                schema: "billing",
                table: "InvoiceLines",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLines_ProductId",
                schema: "billing",
                table: "InvoiceLines",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLines_Products_ProductId",
                schema: "billing",
                table: "InvoiceLines",
                column: "ProductId",
                principalSchema: "inventory",
                principalTable: "Products",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLines_Products_ProductId",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLines_ProductId",
                schema: "billing",
                table: "InvoiceLines");

            migrationBuilder.DropColumn(
                name: "ProductId",
                schema: "billing",
                table: "InvoiceLines");
        }
    }
}
