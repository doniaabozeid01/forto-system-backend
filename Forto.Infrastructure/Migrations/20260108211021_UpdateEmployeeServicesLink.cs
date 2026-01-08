using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeServicesLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeServices",
                schema: "hr",
                table: "EmployeeServices");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                schema: "hr",
                table: "EmployeeServices",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "hr",
                table: "EmployeeServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "hr",
                table: "EmployeeServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "hr",
                table: "EmployeeServices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeServices",
                schema: "hr",
                table: "EmployeeServices",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeServices_EmployeeId_ServiceId",
                schema: "hr",
                table: "EmployeeServices",
                columns: new[] { "EmployeeId", "ServiceId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_EmployeeServices",
                schema: "hr",
                table: "EmployeeServices");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeServices_EmployeeId_ServiceId",
                schema: "hr",
                table: "EmployeeServices");

            migrationBuilder.DropColumn(
                name: "Id",
                schema: "hr",
                table: "EmployeeServices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "hr",
                table: "EmployeeServices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "hr",
                table: "EmployeeServices");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "hr",
                table: "EmployeeServices");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmployeeServices",
                schema: "hr",
                table: "EmployeeServices",
                columns: new[] { "EmployeeId", "ServiceId" });
        }
    }
}
