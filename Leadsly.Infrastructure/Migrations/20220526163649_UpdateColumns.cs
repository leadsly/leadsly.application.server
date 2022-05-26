using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SearchUrlsProgress",
                columns: table => new
                {
                    SearchUrlProgressId = table.Column<string>(type: "text", nullable: false),
                    WindowHandleId = table.Column<string>(type: "text", nullable: false),
                    LastPage = table.Column<int>(type: "integer", nullable: false),
                    Exhausted = table.Column<bool>(type: "boolean", nullable: false),
                    StartedCrawling = table.Column<bool>(type: "boolean", nullable: false),
                    LastActivityTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    LastProcessedProspect = table.Column<int>(type: "integer", nullable: false),
                    SearchUrl = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchUrlsProgress", x => x.SearchUrlProgressId);
                    table.ForeignKey(
                        name: "FK_SearchUrlsProgress_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SearchUrlsProgress_CampaignId",
                table: "SearchUrlsProgress",
                column: "CampaignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SearchUrlsProgress");
        }
    }
}
