using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class AddFollowUpmessageJobTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowUpMessageJobs",
                columns: table => new
                {
                    FollowUpMessageJobId = table.Column<string>(type: "text", nullable: false),
                    CampaignProspectId = table.Column<string>(type: "text", nullable: false),
                    FollowUpMessageId = table.Column<string>(type: "text", nullable: false),
                    HangfireJobId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpMessageJobs", x => x.FollowUpMessageJobId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowUpMessageJobs");
        }
    }
}
