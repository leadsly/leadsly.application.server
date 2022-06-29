using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class UpdateEcsServiceColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EcsServices_SocialAccountResources_SocialAccountCloudResour~",
                table: "EcsServices");

            migrationBuilder.DropForeignKey(
                name: "FK_SocialAccountResources_CloudMapServiceDiscoveryServices_Clo~",
                table: "SocialAccountResources");

            migrationBuilder.DropForeignKey(
                name: "FK_VirtualAssistants_EcsServices_EcsServiceId",
                table: "VirtualAssistants");

            migrationBuilder.DropIndex(
                name: "IX_VirtualAssistants_EcsServiceId",
                table: "VirtualAssistants");

            migrationBuilder.DropIndex(
                name: "IX_EcsServices_SocialAccountCloudResourceId",
                table: "EcsServices");

            migrationBuilder.DropColumn(
                name: "EcsServiceId",
                table: "VirtualAssistants");

            migrationBuilder.DropColumn(
                name: "SocialAccountCloudResourceId",
                table: "EcsServices");

            migrationBuilder.RenameColumn(
                name: "CloudMapDiscoveryServiceCloudMapServiceDiscoveryServiceId",
                table: "VirtualAssistants",
                newName: "CloudMapDiscoveryServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_VirtualAssistants_CloudMapDiscoveryServiceCloudMapServiceDi~",
                table: "VirtualAssistants",
                newName: "IX_VirtualAssistants_CloudMapDiscoveryServiceId");

            migrationBuilder.RenameColumn(
                name: "CloudMapServiceDiscoveryServiceId",
                table: "SocialAccountResources",
                newName: "EcsServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_SocialAccountResources_CloudMapServiceDiscoveryServiceId",
                table: "SocialAccountResources",
                newName: "IX_SocialAccountResources_EcsServiceId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EcsServices",
                newName: "VirtualAssistantId");

            migrationBuilder.RenameColumn(
                name: "CloudMapServiceDiscoveryServiceId",
                table: "CloudMapServiceDiscoveryServices",
                newName: "CloudMapDiscoveryServiceId");

            migrationBuilder.AddColumn<string>(
                name: "CloudMapDiscoveryServiceId",
                table: "SocialAccountResources",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SocialAccountResources_CloudMapDiscoveryServiceId",
                table: "SocialAccountResources",
                column: "CloudMapDiscoveryServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_EcsServices_VirtualAssistantId",
                table: "EcsServices",
                column: "VirtualAssistantId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EcsServices_VirtualAssistants_VirtualAssistantId",
                table: "EcsServices",
                column: "VirtualAssistantId",
                principalTable: "VirtualAssistants",
                principalColumn: "VirtualAssistantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SocialAccountResources_CloudMapServiceDiscoveryServices_Clo~",
                table: "SocialAccountResources",
                column: "CloudMapDiscoveryServiceId",
                principalTable: "CloudMapServiceDiscoveryServices",
                principalColumn: "CloudMapDiscoveryServiceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SocialAccountResources_EcsServices_EcsServiceId",
                table: "SocialAccountResources",
                column: "EcsServiceId",
                principalTable: "EcsServices",
                principalColumn: "EcsServiceId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EcsServices_VirtualAssistants_VirtualAssistantId",
                table: "EcsServices");

            migrationBuilder.DropForeignKey(
                name: "FK_SocialAccountResources_CloudMapServiceDiscoveryServices_Clo~",
                table: "SocialAccountResources");

            migrationBuilder.DropForeignKey(
                name: "FK_SocialAccountResources_EcsServices_EcsServiceId",
                table: "SocialAccountResources");

            migrationBuilder.DropIndex(
                name: "IX_SocialAccountResources_CloudMapDiscoveryServiceId",
                table: "SocialAccountResources");

            migrationBuilder.DropIndex(
                name: "IX_EcsServices_VirtualAssistantId",
                table: "EcsServices");

            migrationBuilder.DropColumn(
                name: "CloudMapDiscoveryServiceId",
                table: "SocialAccountResources");

            migrationBuilder.RenameColumn(
                name: "CloudMapDiscoveryServiceId",
                table: "VirtualAssistants",
                newName: "CloudMapDiscoveryServiceCloudMapServiceDiscoveryServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_VirtualAssistants_CloudMapDiscoveryServiceId",
                table: "VirtualAssistants",
                newName: "IX_VirtualAssistants_CloudMapDiscoveryServiceCloudMapServiceDi~");

            migrationBuilder.RenameColumn(
                name: "EcsServiceId",
                table: "SocialAccountResources",
                newName: "CloudMapServiceDiscoveryServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_SocialAccountResources_EcsServiceId",
                table: "SocialAccountResources",
                newName: "IX_SocialAccountResources_CloudMapServiceDiscoveryServiceId");

            migrationBuilder.RenameColumn(
                name: "VirtualAssistantId",
                table: "EcsServices",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "CloudMapDiscoveryServiceId",
                table: "CloudMapServiceDiscoveryServices",
                newName: "CloudMapServiceDiscoveryServiceId");

            migrationBuilder.AddColumn<string>(
                name: "EcsServiceId",
                table: "VirtualAssistants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialAccountCloudResourceId",
                table: "EcsServices",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirtualAssistants_EcsServiceId",
                table: "VirtualAssistants",
                column: "EcsServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_EcsServices_SocialAccountCloudResourceId",
                table: "EcsServices",
                column: "SocialAccountCloudResourceId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EcsServices_SocialAccountResources_SocialAccountCloudResour~",
                table: "EcsServices",
                column: "SocialAccountCloudResourceId",
                principalTable: "SocialAccountResources",
                principalColumn: "SocialAccountCloudResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SocialAccountResources_CloudMapServiceDiscoveryServices_Clo~",
                table: "SocialAccountResources",
                column: "CloudMapServiceDiscoveryServiceId",
                principalTable: "CloudMapServiceDiscoveryServices",
                principalColumn: "CloudMapServiceDiscoveryServiceId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VirtualAssistants_EcsServices_EcsServiceId",
                table: "VirtualAssistants",
                column: "EcsServiceId",
                principalTable: "EcsServices",
                principalColumn: "EcsServiceId");
        }
    }
}
