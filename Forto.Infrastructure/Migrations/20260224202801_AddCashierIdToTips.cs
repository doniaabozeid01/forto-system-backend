using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashierIdToTips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CashierId",
                schema: "billing",
                table: "Tips",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tips_CashierId",
                schema: "billing",
                table: "Tips",
                column: "CashierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tips_Employees_CashierId",
                schema: "billing",
                table: "Tips",
                column: "CashierId",
                principalSchema: "hr",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tips_Employees_CashierId",
                schema: "billing",
                table: "Tips");

            migrationBuilder.DropIndex(
                name: "IX_Tips_CashierId",
                schema: "billing",
                table: "Tips");

            migrationBuilder.DropColumn(
                name: "CashierId",
                schema: "billing",
                table: "Tips");
        }
    }
}
