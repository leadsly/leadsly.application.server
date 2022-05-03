using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class AddCampaignProspectFollowUpMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CampaignProspectFollowUpMessages_CampaignProspectId",
                table: "CampaignProspectFollowUpMessages");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspectFollowUpMessages_CampaignProspectId",
                table: "CampaignProspectFollowUpMessages",
                column: "CampaignProspectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CampaignProspectFollowUpMessages_CampaignProspectId",
                table: "CampaignProspectFollowUpMessages");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspectFollowUpMessages_CampaignProspectId",
                table: "CampaignProspectFollowUpMessages",
                column: "CampaignProspectId",
                unique: true);
        }
    }
}
