using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskly.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoWorkspace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Workspaces",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Demo Workspace" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Workspaces",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
