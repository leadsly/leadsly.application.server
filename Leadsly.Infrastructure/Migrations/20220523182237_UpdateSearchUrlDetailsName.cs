using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateSearchUrlDetailsName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SentConnectionsSearchUrlStatusId",
                table: "SentConnectionsStatuses",
                newName: "SearchUrlDetailsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SearchUrlDetailsId",
                table: "SentConnectionsStatuses",
                newName: "SentConnectionsSearchUrlStatusId");
        }
    }
}
