using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// < inheritdoc />
    public partial class AddShiftIdToCAshierShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShiftId",
                schema: "ops",
                table: "CashierShifts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashierShifts_ShiftId",
                schema: "ops",
                table: "CashierShifts",
                column: "ShiftId");

            migrationBuilder.AddForeignKey(
                name: "FK_CashierShifts_Shifts_ShiftId",
                schema: "ops",
                table: "CashierShifts",
                column: "ShiftId",
                principalSchema: "hr",
                principalTable: "Shifts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CashierShifts_Shifts_ShiftId",
                schema: "ops",
                table: "CashierShifts");

            migrationBuilder.DropIndex(
                name: "IX_CashierShifts_ShiftId",
                schema: "ops",
                table: "CashierShifts");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                schema: "ops",
                table: "CashierShifts");
        }
    }
}
