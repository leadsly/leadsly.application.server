using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "PrimaryProspects",
                newName: "SearchResultAvatarUrl");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "PrimaryProspects",
                newName: "EmploymentInfo");

            migrationBuilder.RenameColumn(
                name: "AddedByCampaignId",
                table: "PrimaryProspects",
                newName: "Area");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SearchResultAvatarUrl",
                table: "PrimaryProspects",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "EmploymentInfo",
                table: "PrimaryProspects",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Area",
                table: "PrimaryProspects",
                newName: "AddedByCampaignId");
        }
    }
}
