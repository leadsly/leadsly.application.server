using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class AddColumnsToCampaignProspectFollowUpMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ActualDeliveryDateTimeStamp",
                table: "CampaignProspectFollowUpMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "ExpectedDeliveryDateTimeStamp",
                table: "CampaignProspectFollowUpMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualDeliveryDateTimeStamp",
                table: "CampaignProspectFollowUpMessages");

            migrationBuilder.DropColumn(
                name: "ExpectedDeliveryDateTimeStamp",
                table: "CampaignProspectFollowUpMessages");
        }
    }
}
