using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateECSTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FollowUpMessagesPhases_Campaigns_CampaignId",
                table: "FollowUpMessagesPhases");

            migrationBuilder.DropForeignKey(
                name: "FK_ProspectListPhases_Campaigns_CampaignId",
                table: "ProspectListPhases");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchUrlsProgress_Campaigns_CampaignId",
                table: "SearchUrlsProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_SendConnectionRequestPhases_Campaigns_CampaignId",
                table: "SendConnectionRequestPhases");

            migrationBuilder.DropForeignKey(
                name: "FK_SendConnectionsStages_Campaigns_CampaignId",
                table: "SendConnectionsStages");

            migrationBuilder.DropForeignKey(
                name: "FK_SentConnectionsStatuses_Campaigns_CampaignId",
                table: "SentConnectionsStatuses");

            migrationBuilder.DropForeignKey(
                name: "FK_VirtualAssistants_CloudMapDiscoveryServices_CloudMapDiscove~",
                table: "VirtualAssistants");

            migrationBuilder.DropForeignKey(
                name: "FK_VirtualAssistants_EcsServices_EcsServiceId",
                table: "VirtualAssistants");

            migrationBuilder.DropForeignKey(
                name: "FK_VirtualAssistants_EcsTaskDefinitions_EcsTaskDefinitionId",
                table: "VirtualAssistants");

            migrationBuilder.DropIndex(
                name: "IX_VirtualAssistants_CloudMapDiscoveryServiceId",
                table: "VirtualAssistants");

            migrationBuilder.DropIndex(
                name: "IX_VirtualAssistants_EcsServiceId",
                table: "VirtualAssistants");

            migrationBuilder.DropIndex(
                name: "IX_VirtualAssistants_EcsTaskDefinitionId",
                table: "VirtualAssistants");

            migrationBuilder.DropColumn(
                name: "CloudMapDiscoveryServiceId",
                table: "VirtualAssistants");

            migrationBuilder.DropColumn(
                name: "EcsServiceId",
                table: "VirtualAssistants");

            migrationBuilder.DropColumn(
                name: "EcsTaskDefinitionId",
                table: "VirtualAssistants");

            migrationBuilder.DropColumn(
                name: "ContainerPurpose",
                table: "EcsTasks");

            migrationBuilder.AddColumn<string>(
                name: "VirtualAssistantId",
                table: "EcsTaskDefinitions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Purpose",
                table: "EcsServices",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VirtualAssistantId",
                table: "EcsServices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VirtualAssistantId",
                table: "CloudMapDiscoveryServices",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcsTaskDefinitions_VirtualAssistantId",
                table: "EcsTaskDefinitions",
                column: "VirtualAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_EcsServices_VirtualAssistantId",
                table: "EcsServices",
                column: "VirtualAssistantId");

            migrationBuilder.CreateIndex(
                name: "IX_CloudMapDiscoveryServices_VirtualAssistantId",
                table: "CloudMapDiscoveryServices",
                column: "VirtualAssistantId");

            migrationBuilder.AddForeignKey(
                name: "FK_CloudMapDiscoveryServices_VirtualAssistants_VirtualAssistan~",
                table: "CloudMapDiscoveryServices",
                column: "VirtualAssistantId",
                principalTable: "VirtualAssistants",
                principalColumn: "VirtualAssistantId");

            migrationBuilder.AddForeignKey(
                name: "FK_EcsServices_VirtualAssistants_VirtualAssistantId",
                table: "EcsServices",
                column: "VirtualAssistantId",
                principalTable: "VirtualAssistants",
                principalColumn: "VirtualAssistantId");

            migrationBuilder.AddForeignKey(
                name: "FK_EcsTaskDefinitions_VirtualAssistants_VirtualAssistantId",
                table: "EcsTaskDefinitions",
                column: "VirtualAssistantId",
                principalTable: "VirtualAssistants",
                principalColumn: "VirtualAssistantId");

            migrationBuilder.AddForeignKey(
                name: "FK_FollowUpMessagesPhases_Campaigns_CampaignId",
                table: "FollowUpMessagesPhases",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProspectListPhases_Campaigns_CampaignId",
                table: "ProspectListPhases",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchUrlsProgress_Campaigns_CampaignId",
                table: "SearchUrlsProgress",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SendConnectionRequestPhases_Campaigns_CampaignId",
                table: "SendConnectionRequestPhases",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SendConnectionsStages_Campaigns_CampaignId",
                table: "SendConnectionsStages",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SentConnectionsStatuses_Campaigns_CampaignId",
                table: "SentConnectionsStatuses",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CloudMapDiscoveryServices_VirtualAssistants_VirtualAssistan~",
                table: "CloudMapDiscoveryServices");

            migrationBuilder.DropForeignKey(
                name: "FK_EcsServices_VirtualAssistants_VirtualAssistantId",
                table: "EcsServices");

            migrationBuilder.DropForeignKey(
                name: "FK_EcsTaskDefinitions_VirtualAssistants_VirtualAssistantId",
                table: "EcsTaskDefinitions");

            migrationBuilder.DropForeignKey(
                name: "FK_FollowUpMessagesPhases_Campaigns_CampaignId",
                table: "FollowUpMessagesPhases");

            migrationBuilder.DropForeignKey(
                name: "FK_ProspectListPhases_Campaigns_CampaignId",
                table: "ProspectListPhases");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchUrlsProgress_Campaigns_CampaignId",
                table: "SearchUrlsProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_SendConnectionRequestPhases_Campaigns_CampaignId",
                table: "SendConnectionRequestPhases");

            migrationBuilder.DropForeignKey(
                name: "FK_SendConnectionsStages_Campaigns_CampaignId",
                table: "SendConnectionsStages");

            migrationBuilder.DropForeignKey(
                name: "FK_SentConnectionsStatuses_Campaigns_CampaignId",
                table: "SentConnectionsStatuses");

            migrationBuilder.DropIndex(
                name: "IX_EcsTaskDefinitions_VirtualAssistantId",
                table: "EcsTaskDefinitions");

            migrationBuilder.DropIndex(
                name: "IX_EcsServices_VirtualAssistantId",
                table: "EcsServices");

            migrationBuilder.DropIndex(
                name: "IX_CloudMapDiscoveryServices_VirtualAssistantId",
                table: "CloudMapDiscoveryServices");

            migrationBuilder.DropColumn(
                name: "VirtualAssistantId",
                table: "EcsTaskDefinitions");

            migrationBuilder.DropColumn(
                name: "Purpose",
                table: "EcsServices");

            migrationBuilder.DropColumn(
                name: "VirtualAssistantId",
                table: "EcsServices");

            migrationBuilder.DropColumn(
                name: "VirtualAssistantId",
                table: "CloudMapDiscoveryServices");

            migrationBuilder.AddColumn<string>(
                name: "CloudMapDiscoveryServiceId",
                table: "VirtualAssistants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EcsServiceId",
                table: "VirtualAssistants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EcsTaskDefinitionId",
                table: "VirtualAssistants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContainerPurpose",
                table: "EcsTasks",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualAssistants_CloudMapDiscoveryServiceId",
                table: "VirtualAssistants",
                column: "CloudMapDiscoveryServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualAssistants_EcsServiceId",
                table: "VirtualAssistants",
                column: "EcsServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VirtualAssistants_EcsTaskDefinitionId",
                table: "VirtualAssistants",
                column: "EcsTaskDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_FollowUpMessagesPhases_Campaigns_CampaignId",
                table: "FollowUpMessagesPhases",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProspectListPhases_Campaigns_CampaignId",
                table: "ProspectListPhases",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_SearchUrlsProgress_Campaigns_CampaignId",
                table: "SearchUrlsProgress",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_SendConnectionRequestPhases_Campaigns_CampaignId",
                table: "SendConnectionRequestPhases",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_SendConnectionsStages_Campaigns_CampaignId",
                table: "SendConnectionsStages",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_SentConnectionsStatuses_Campaigns_CampaignId",
                table: "SentConnectionsStatuses",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_VirtualAssistants_CloudMapDiscoveryServices_CloudMapDiscove~",
                table: "VirtualAssistants",
                column: "CloudMapDiscoveryServiceId",
                principalTable: "CloudMapDiscoveryServices",
                principalColumn: "CloudMapDiscoveryServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_VirtualAssistants_EcsServices_EcsServiceId",
                table: "VirtualAssistants",
                column: "EcsServiceId",
                principalTable: "EcsServices",
                principalColumn: "EcsServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_VirtualAssistants_EcsTaskDefinitions_EcsTaskDefinitionId",
                table: "VirtualAssistants",
                column: "EcsTaskDefinitionId",
                principalTable: "EcsTaskDefinitions",
                principalColumn: "EcsTaskDefinitionId");
        }
    }
}
