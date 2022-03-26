using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateTableName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrimaryProspectLists_Users_ApplicationUserId",
                table: "PrimaryProspectLists");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "PrimaryProspectLists",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_PrimaryProspectLists_ApplicationUserId",
                table: "PrimaryProspectLists",
                newName: "IX_PrimaryProspectLists_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrimaryProspectLists_Users_UserId",
                table: "PrimaryProspectLists",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrimaryProspectLists_Users_UserId",
                table: "PrimaryProspectLists");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PrimaryProspectLists",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_PrimaryProspectLists_UserId",
                table: "PrimaryProspectLists",
                newName: "IX_PrimaryProspectLists_ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrimaryProspectLists_Users_ApplicationUserId",
                table: "PrimaryProspectLists",
                column: "ApplicationUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
