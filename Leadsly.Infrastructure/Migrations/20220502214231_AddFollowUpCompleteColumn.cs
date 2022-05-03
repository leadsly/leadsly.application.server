using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class AddFollowUpCompleteColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignProspects_CampaignProspectLists_CampaignProspectLis~",
                table: "CampaignProspects");

            migrationBuilder.AlterColumn<string>(
                name: "CampaignProspectListId",
                table: "CampaignProspects",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FollowUpComplete",
                table: "CampaignProspects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignProspects_CampaignProspectLists_CampaignProspectLis~",
                table: "CampaignProspects",
                column: "CampaignProspectListId",
                principalTable: "CampaignProspectLists",
                principalColumn: "CampaignProspectListId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignProspects_CampaignProspectLists_CampaignProspectLis~",
                table: "CampaignProspects");

            migrationBuilder.DropColumn(
                name: "FollowUpComplete",
                table: "CampaignProspects");

            migrationBuilder.AlterColumn<string>(
                name: "CampaignProspectListId",
                table: "CampaignProspects",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignProspects_CampaignProspectLists_CampaignProspectLis~",
                table: "CampaignProspects",
                column: "CampaignProspectListId",
                principalTable: "CampaignProspectLists",
                principalColumn: "CampaignProspectListId");
        }
    }
}
