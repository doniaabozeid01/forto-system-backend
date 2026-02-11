using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceDeletionRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletionReason",
                schema: "billing",
                table: "Invoices",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionRejectedAt",
                schema: "billing",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionRequestedAt",
                schema: "billing",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletionRequestedByEmployeeId",
                schema: "billing",
                table: "Invoices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletionReason",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DeletionRejectedAt",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DeletionRequestedAt",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DeletionRequestedByEmployeeId",
                schema: "billing",
                table: "Invoices");
        }
    }
}
