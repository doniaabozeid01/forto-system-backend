using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceSupervisorId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupervisorId",
                schema: "billing",
                table: "Invoices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupervisorId",
                schema: "billing",
                table: "Invoices");
        }
    }
}
