using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class AddWindowHandleIdColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WindowHandleId",
                table: "SentConnectionsStatuses",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WindowHandleId",
                table: "SentConnectionsStatuses");
        }
    }
}
