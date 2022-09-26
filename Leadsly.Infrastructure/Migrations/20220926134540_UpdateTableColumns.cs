using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateTableColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConnectionWithdrawPhases");

            migrationBuilder.DropTable(
                name: "SendConnectionRequestPhases");

            migrationBuilder.DropTable(
                name: "SendEmailInvitePhases");

            migrationBuilder.AddColumn<int>(
                name: "TotalConnectionsCount",
                table: "SocialAccounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TaskDefinitionArn",
                table: "EcsTaskDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Family",
                table: "EcsTaskDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HalId",
                table: "EcsTaskDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "EcsTaskDefinitions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HalId",
                table: "EcsServices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "NamespaceId",
                table: "CloudMapDiscoveryServices",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CloudMapDiscoveryServices",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "CloudMapDiscoveryServices",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HalId",
                table: "CloudMapDiscoveryServices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "CloudMapDiscoveryServices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "FollowUpCompleteTimestamp",
                table: "CampaignProspects",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "RecentlyAddedProspects",
                columns: table => new
                {
                    RecentlyAddedProspectId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ProfileUrl = table.Column<string>(type: "text", nullable: true),
                    AcceptedRequestTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    SocialAccountId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecentlyAddedProspects", x => x.RecentlyAddedProspectId);
                    table.ForeignKey(
                        name: "FK_RecentlyAddedProspects_SocialAccounts_SocialAccountId",
                        column: x => x.SocialAccountId,
                        principalTable: "SocialAccounts",
                        principalColumn: "SocialAccountId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecentlyAddedProspects_SocialAccountId",
                table: "RecentlyAddedProspects",
                column: "SocialAccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecentlyAddedProspects");

            migrationBuilder.DropColumn(
                name: "TotalConnectionsCount",
                table: "SocialAccounts");

            migrationBuilder.DropColumn(
                name: "HalId",
                table: "EcsTaskDefinitions");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "EcsTaskDefinitions");

            migrationBuilder.DropColumn(
                name: "HalId",
                table: "EcsServices");

            migrationBuilder.DropColumn(
                name: "HalId",
                table: "CloudMapDiscoveryServices");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "CloudMapDiscoveryServices");

            migrationBuilder.DropColumn(
                name: "FollowUpCompleteTimestamp",
                table: "CampaignProspects");

            migrationBuilder.AlterColumn<string>(
                name: "TaskDefinitionArn",
                table: "EcsTaskDefinitions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Family",
                table: "EcsTaskDefinitions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "NamespaceId",
                table: "CloudMapDiscoveryServices",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CloudMapDiscoveryServices",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreateDate",
                table: "CloudMapDiscoveryServices",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "ConnectionWithdrawPhases",
                columns: table => new
                {
                    ConnectionWithdrawPhaseId = table.Column<string>(type: "text", nullable: false),
                    SocialAccountId = table.Column<string>(type: "text", nullable: true),
                    PageUrl = table.Column<string>(type: "text", nullable: true),
                    PhaseType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConnectionWithdrawPhases", x => x.ConnectionWithdrawPhaseId);
                    table.ForeignKey(
                        name: "FK_ConnectionWithdrawPhases_SocialAccounts_SocialAccountId",
                        column: x => x.SocialAccountId,
                        principalTable: "SocialAccounts",
                        principalColumn: "SocialAccountId");
                });

            migrationBuilder.CreateTable(
                name: "SendConnectionRequestPhases",
                columns: table => new
                {
                    SendConnectionRequestPhaseId = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true),
                    PhaseType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendConnectionRequestPhases", x => x.SendConnectionRequestPhaseId);
                    table.ForeignKey(
                        name: "FK_SendConnectionRequestPhases_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SendEmailInvitePhases",
                columns: table => new
                {
                    SendEmailInvitePhaseId = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true),
                    PhaseType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendEmailInvitePhases", x => x.SendEmailInvitePhaseId);
                    table.ForeignKey(
                        name: "FK_SendEmailInvitePhases_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionWithdrawPhases_SocialAccountId",
                table: "ConnectionWithdrawPhases",
                column: "SocialAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SendConnectionRequestPhases_CampaignId",
                table: "SendConnectionRequestPhases",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SendEmailInvitePhases_CampaignId",
                table: "SendEmailInvitePhases",
                column: "CampaignId",
                unique: true);
        }
    }
}
