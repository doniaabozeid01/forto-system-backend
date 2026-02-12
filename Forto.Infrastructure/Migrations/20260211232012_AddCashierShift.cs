using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashierShift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CashierShiftId",
                schema: "billing",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CashierShifts",
                schema: "ops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    OpenedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierShifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashierShifts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "ops",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CashierShifts_Employees_ClosedByEmployeeId",
                        column: x => x.ClosedByEmployeeId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CashierShifts_Employees_OpenedByEmployeeId",
                        column: x => x.OpenedByEmployeeId,
                        principalSchema: "hr",
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CashierShiftId",
                schema: "billing",
                table: "Invoices",
                column: "CashierShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierShifts_BranchId",
                schema: "ops",
                table: "CashierShifts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierShifts_ClosedByEmployeeId",
                schema: "ops",
                table: "CashierShifts",
                column: "ClosedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierShifts_OpenedByEmployeeId",
                schema: "ops",
                table: "CashierShifts",
                column: "OpenedByEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_CashierShifts_CashierShiftId",
                schema: "billing",
                table: "Invoices",
                column: "CashierShiftId",
                principalSchema: "ops",
                principalTable: "CashierShifts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_CashierShifts_CashierShiftId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "CashierShifts",
                schema: "ops");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CashierShiftId",
                schema: "billing",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CashierShiftId",
                schema: "billing",
                table: "Invoices");
        }
    }
}
