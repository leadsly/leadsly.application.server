using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateSchema4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignProspects_PrimaryProspects_Id",
                table: "CampaignProspects");

            migrationBuilder.DropColumn(
                name: "CompletionPercentage",
                table: "ProspectListPhases");

            migrationBuilder.AddColumn<bool>(
                name: "Completed",
                table: "ProspectListPhases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CampaignId",
                table: "CampaignProspects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryProspectId",
                table: "CampaignProspects",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CampaignWarmUps",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    DailyLimit = table.Column<int>(type: "integer", nullable: false),
                    StartDateTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignWarmUps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CampaignWarmUps_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SendConnectionsStages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<string>(type: "text", nullable: false),
                    NumOfConnections = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendConnectionsStages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SendConnectionsStages_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspects_CampaignId",
                table: "CampaignProspects",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspects_PrimaryProspectId",
                table: "CampaignProspects",
                column: "PrimaryProspectId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignWarmUps_CampaignId",
                table: "CampaignWarmUps",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SendConnectionsStages_CampaignId",
                table: "SendConnectionsStages",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignProspects_Campaigns_CampaignId",
                table: "CampaignProspects",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignProspects_PrimaryProspects_PrimaryProspectId",
                table: "CampaignProspects",
                column: "PrimaryProspectId",
                principalTable: "PrimaryProspects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CampaignProspects_Campaigns_CampaignId",
                table: "CampaignProspects");

            migrationBuilder.DropForeignKey(
                name: "FK_CampaignProspects_PrimaryProspects_PrimaryProspectId",
                table: "CampaignProspects");

            migrationBuilder.DropTable(
                name: "CampaignWarmUps");

            migrationBuilder.DropTable(
                name: "SendConnectionsStages");

            migrationBuilder.DropIndex(
                name: "IX_CampaignProspects_CampaignId",
                table: "CampaignProspects");

            migrationBuilder.DropIndex(
                name: "IX_CampaignProspects_PrimaryProspectId",
                table: "CampaignProspects");

            migrationBuilder.DropColumn(
                name: "Completed",
                table: "ProspectListPhases");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "CampaignProspects");

            migrationBuilder.DropColumn(
                name: "PrimaryProspectId",
                table: "CampaignProspects");

            migrationBuilder.AddColumn<int>(
                name: "CompletionPercentage",
                table: "ProspectListPhases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_CampaignProspects_PrimaryProspects_Id",
                table: "CampaignProspects",
                column: "Id",
                principalTable: "PrimaryProspects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
