using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientToInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                schema: "billing",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "billing",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerPhone",
                schema: "billing",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClientId",
                schema: "billing",
                table: "Invoices",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Clients_ClientId",
                schema: "billing",
                table: "Invoices",
                column: "ClientId",
                principalSchema: "crm",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Clients_ClientId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ClientId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ClientId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CustomerPhone",
                schema: "billing",
                table: "Invoices");
        }
    }
}
