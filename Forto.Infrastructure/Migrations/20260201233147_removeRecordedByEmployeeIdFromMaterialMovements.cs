using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Forto.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class removeRecordedByEmployeeIdFromMaterialMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordedByEmployeeId",
                schema: "ops",
                table: "MaterialMovements");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecordedByEmployeeId",
                schema: "ops",
                table: "MaterialMovements",
                type: "int",
                nullable: true);
        }
    }
}
