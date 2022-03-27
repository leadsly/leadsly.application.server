using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateSchema1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SentConnectionsStatus_SearchUrls_LastVisistedPageUrlId",
                table: "SentConnectionsStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_SentConnectionsStatus_SearchUrls_NextPageUrlId",
                table: "SentConnectionsStatus");

            migrationBuilder.AlterColumn<string>(
                name: "NextPageUrlId",
                table: "SentConnectionsStatus",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastVisistedPageUrlId",
                table: "SentConnectionsStatus",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_SentConnectionsStatus_SearchUrls_LastVisistedPageUrlId",
                table: "SentConnectionsStatus",
                column: "LastVisistedPageUrlId",
                principalTable: "SearchUrls",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SentConnectionsStatus_SearchUrls_NextPageUrlId",
                table: "SentConnectionsStatus",
                column: "NextPageUrlId",
                principalTable: "SearchUrls",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SentConnectionsStatus_SearchUrls_LastVisistedPageUrlId",
                table: "SentConnectionsStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_SentConnectionsStatus_SearchUrls_NextPageUrlId",
                table: "SentConnectionsStatus");

            migrationBuilder.AlterColumn<string>(
                name: "NextPageUrlId",
                table: "SentConnectionsStatus",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LastVisistedPageUrlId",
                table: "SentConnectionsStatus",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SentConnectionsStatus_SearchUrls_LastVisistedPageUrlId",
                table: "SentConnectionsStatus",
                column: "LastVisistedPageUrlId",
                principalTable: "SearchUrls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SentConnectionsStatus_SearchUrls_NextPageUrlId",
                table: "SentConnectionsStatus",
                column: "NextPageUrlId",
                principalTable: "SearchUrls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
