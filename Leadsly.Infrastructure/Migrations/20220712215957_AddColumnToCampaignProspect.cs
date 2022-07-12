using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class AddColumnToCampaignProspect : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CreatedTimestamp",
                table: "Campaigns",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RepliedTimestamp",
                table: "CampaignProspects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedTimestamp",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "RepliedTimestamp",
                table: "CampaignProspects");
        }
    }
}
