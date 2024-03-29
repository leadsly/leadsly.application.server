﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Leadsly.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChromeProfileNames",
                columns: table => new
                {
                    ChromeProfileId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CampaignPhaseType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChromeProfileNames", x => x.ChromeProfileId);
                });

            migrationBuilder.CreateTable(
                name: "EcsServices",
                columns: table => new
                {
                    EcsServiceId = table.Column<string>(type: "text", nullable: false),
                    ServiceName = table.Column<string>(type: "text", nullable: false),
                    ServiceArn = table.Column<string>(type: "text", nullable: false),
                    ClusterArn = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    TaskDefinition = table.Column<string>(type: "text", nullable: false),
                    SchedulingStrategy = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcsServices", x => x.EcsServiceId);
                });

            migrationBuilder.CreateTable(
                name: "EcsTaskDefinitions",
                columns: table => new
                {
                    EcsTaskDefinitionId = table.Column<string>(type: "text", nullable: false),
                    TaskDefinitionArn = table.Column<string>(type: "text", nullable: true),
                    Family = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcsTaskDefinitions", x => x.EcsTaskDefinitionId);
                });

            migrationBuilder.CreateTable(
                name: "FollowUpMessageJobs",
                columns: table => new
                {
                    FollowUpMessageJobId = table.Column<string>(type: "text", nullable: false),
                    CampaignProspectId = table.Column<string>(type: "text", nullable: true),
                    FollowUpMessageId = table.Column<string>(type: "text", nullable: true),
                    HangfireJobId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpMessageJobs", x => x.FollowUpMessageJobId);
                });

            migrationBuilder.CreateTable(
                name: "OrphanedCloudResources",
                columns: table => new
                {
                    OrphanedCloudResourceId = table.Column<string>(type: "text", nullable: false),
                    Arn = table.Column<string>(type: "text", nullable: true),
                    FriendlyName = table.Column<string>(type: "text", nullable: true),
                    ResourceId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    ResourceName = table.Column<string>(type: "text", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrphanedCloudResources", x => x.OrphanedCloudResourceId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StripeCustomers",
                columns: table => new
                {
                    Customer_StripeId = table.Column<string>(type: "text", nullable: false),
                    Customer = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StripeCustomers", x => x.Customer_StripeId);
                });

            migrationBuilder.CreateTable(
                name: "SupportedTimeZones",
                columns: table => new
                {
                    LeadslyTimeZoneId = table.Column<string>(type: "text", nullable: false),
                    TimeZoneId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportedTimeZones", x => x.LeadslyTimeZoneId);
                });

            migrationBuilder.CreateTable(
                name: "CloudMapDiscoveryServices",
                columns: table => new
                {
                    CloudMapDiscoveryServiceId = table.Column<string>(type: "text", nullable: false),
                    ServiceDiscoveryId = table.Column<string>(type: "text", nullable: true),
                    Arn = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    NamespaceId = table.Column<string>(type: "text", nullable: true),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EcsServiceId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudMapDiscoveryServices", x => x.CloudMapDiscoveryServiceId);
                    table.ForeignKey(
                        name: "FK_CloudMapDiscoveryServices_EcsServices_EcsServiceId",
                        column: x => x.EcsServiceId,
                        principalTable: "EcsServices",
                        principalColumn: "EcsServiceId");
                });

            migrationBuilder.CreateTable(
                name: "EcsServiceRegistries",
                columns: table => new
                {
                    EcsServiceRegistryId = table.Column<string>(type: "text", nullable: false),
                    EcsServiceId = table.Column<string>(type: "text", nullable: true),
                    RegistryArn = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcsServiceRegistries", x => x.EcsServiceRegistryId);
                    table.ForeignKey(
                        name: "FK_EcsServiceRegistries_EcsServices_EcsServiceId",
                        column: x => x.EcsServiceId,
                        principalTable: "EcsServices",
                        principalColumn: "EcsServiceId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EcsTasks",
                columns: table => new
                {
                    EcsTaskId = table.Column<string>(type: "text", nullable: false),
                    TaskArn = table.Column<string>(type: "text", nullable: true),
                    ContainerName = table.Column<string>(type: "text", nullable: true),
                    EcsServiceId = table.Column<string>(type: "text", nullable: true),
                    ContainerPurpose = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcsTasks", x => x.EcsTaskId);
                    table.ForeignKey(
                        name: "FK_EcsTasks_EcsServices_EcsServiceId",
                        column: x => x.EcsServiceId,
                        principalTable: "EcsServices",
                        principalColumn: "EcsServiceId");
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: true),
                    LastName = table.Column<string>(type: "text", nullable: true),
                    StripeCustomerId = table.Column<string>(type: "text", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    ExternalProviderUserId = table.Column<string>(type: "text", nullable: true),
                    PhotoUrl = table.Column<string>(type: "text", nullable: true),
                    ExternalProvider = table.Column<string>(type: "text", nullable: true),
                    Customer_StripeId = table.Column<string>(type: "text", nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_StripeCustomers_Customer_StripeId",
                        column: x => x.Customer_StripeId,
                        principalTable: "StripeCustomers",
                        principalColumn: "Customer_StripeId");
                });

            migrationBuilder.CreateTable(
                name: "HalUnits",
                columns: table => new
                {
                    HalUnitId = table.Column<string>(type: "text", nullable: false),
                    HalId = table.Column<string>(type: "text", nullable: true),
                    StartHour = table.Column<string>(type: "text", nullable: true),
                    EndHour = table.Column<string>(type: "text", nullable: true),
                    TimeZoneId = table.Column<string>(type: "text", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HalUnits", x => x.HalUnitId);
                    table.ForeignKey(
                        name: "FK_HalUnits_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PrimaryProspectLists",
                columns: table => new
                {
                    PrimaryProspectListId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaryProspectLists", x => x.PrimaryProspectListId);
                    table.ForeignKey(
                        name: "FK_PrimaryProspectLists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VirtualAssistants",
                columns: table => new
                {
                    VirtualAssistantId = table.Column<string>(type: "text", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    HalId = table.Column<string>(type: "text", nullable: false),
                    HalUnitId = table.Column<string>(type: "text", nullable: true),
                    EcsServiceId = table.Column<string>(type: "text", nullable: true),
                    EcsTaskDefinitionId = table.Column<string>(type: "text", nullable: true),
                    CloudMapDiscoveryServiceId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VirtualAssistants", x => x.VirtualAssistantId);
                    table.ForeignKey(
                        name: "FK_VirtualAssistants_CloudMapDiscoveryServices_CloudMapDiscove~",
                        column: x => x.CloudMapDiscoveryServiceId,
                        principalTable: "CloudMapDiscoveryServices",
                        principalColumn: "CloudMapDiscoveryServiceId");
                    table.ForeignKey(
                        name: "FK_VirtualAssistants_EcsServices_EcsServiceId",
                        column: x => x.EcsServiceId,
                        principalTable: "EcsServices",
                        principalColumn: "EcsServiceId");
                    table.ForeignKey(
                        name: "FK_VirtualAssistants_EcsTaskDefinitions_EcsTaskDefinitionId",
                        column: x => x.EcsTaskDefinitionId,
                        principalTable: "EcsTaskDefinitions",
                        principalColumn: "EcsTaskDefinitionId");
                    table.ForeignKey(
                        name: "FK_VirtualAssistants_HalUnits_HalUnitId",
                        column: x => x.HalUnitId,
                        principalTable: "HalUnits",
                        principalColumn: "HalUnitId");
                    table.ForeignKey(
                        name: "FK_VirtualAssistants_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampaignProspectLists",
                columns: table => new
                {
                    CampaignProspectListId = table.Column<string>(type: "text", nullable: false),
                    PrimaryProspectListId = table.Column<string>(type: "text", nullable: true),
                    ProspectListName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignProspectLists", x => x.CampaignProspectListId);
                    table.ForeignKey(
                        name: "FK_CampaignProspectLists_PrimaryProspectLists_PrimaryProspectL~",
                        column: x => x.PrimaryProspectListId,
                        principalTable: "PrimaryProspectLists",
                        principalColumn: "PrimaryProspectListId");
                });

            migrationBuilder.CreateTable(
                name: "PrimaryProspects",
                columns: table => new
                {
                    PrimaryProspectId = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ProfileUrl = table.Column<string>(type: "text", nullable: true),
                    AddedTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    PrimaryProspectListId = table.Column<string>(type: "text", nullable: true),
                    Area = table.Column<string>(type: "text", nullable: true),
                    EmploymentInfo = table.Column<string>(type: "text", nullable: true),
                    SearchResultAvatarUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaryProspects", x => x.PrimaryProspectId);
                    table.ForeignKey(
                        name: "FK_PrimaryProspects_PrimaryProspectLists_PrimaryProspectListId",
                        column: x => x.PrimaryProspectListId,
                        principalTable: "PrimaryProspectLists",
                        principalColumn: "PrimaryProspectListId");
                });

            migrationBuilder.CreateTable(
                name: "SocialAccounts",
                columns: table => new
                {
                    SocialAccountId = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    VirtualAssistantId = table.Column<string>(type: "text", nullable: true),
                    HalUnitId = table.Column<string>(type: "text", nullable: true),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    RunProspectListFirst = table.Column<bool>(type: "boolean", nullable: false),
                    MonthlySearchLimitReached = table.Column<bool>(type: "boolean", nullable: false),
                    Linked = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialAccounts", x => x.SocialAccountId);
                    table.ForeignKey(
                        name: "FK_SocialAccounts_HalUnits_HalUnitId",
                        column: x => x.HalUnitId,
                        principalTable: "HalUnits",
                        principalColumn: "HalUnitId");
                    table.ForeignKey(
                        name: "FK_SocialAccounts_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SocialAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SocialAccounts_VirtualAssistants_VirtualAssistantId",
                        column: x => x.VirtualAssistantId,
                        principalTable: "VirtualAssistants",
                        principalColumn: "VirtualAssistantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    CampaignId = table.Column<string>(type: "text", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: true),
                    HalId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    Expired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    StartTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    EndTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DailyInvites = table.Column<int>(type: "integer", nullable: false),
                    CampaignType = table.Column<int>(type: "integer", nullable: false),
                    IsWarmUpEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CampaignProspectListId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.CampaignId);
                    table.ForeignKey(
                        name: "FK_Campaigns_CampaignProspectLists_CampaignProspectListId",
                        column: x => x.CampaignProspectListId,
                        principalTable: "CampaignProspectLists",
                        principalColumn: "CampaignProspectListId");
                    table.ForeignKey(
                        name: "FK_Campaigns_Users_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SearchUrls",
                columns: table => new
                {
                    SearchUrlId = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: true),
                    PrimaryProspectListId = table.Column<string>(type: "text", nullable: true),
                    CampaignProspectListId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchUrls", x => x.SearchUrlId);
                    table.ForeignKey(
                        name: "FK_SearchUrls_CampaignProspectLists_CampaignProspectListId",
                        column: x => x.CampaignProspectListId,
                        principalTable: "CampaignProspectLists",
                        principalColumn: "CampaignProspectListId");
                    table.ForeignKey(
                        name: "FK_SearchUrls_PrimaryProspectLists_PrimaryProspectListId",
                        column: x => x.PrimaryProspectListId,
                        principalTable: "PrimaryProspectLists",
                        principalColumn: "PrimaryProspectListId");
                });

            migrationBuilder.CreateTable(
                name: "ConnectionWithdrawPhases",
                columns: table => new
                {
                    ConnectionWithdrawPhaseId = table.Column<string>(type: "text", nullable: false),
                    PhaseType = table.Column<int>(type: "integer", nullable: false),
                    PageUrl = table.Column<string>(type: "text", nullable: true),
                    SocialAccountId = table.Column<string>(type: "text", nullable: true)
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
                name: "MonitorForNewConnectionsPhases",
                columns: table => new
                {
                    MonitorForNewConnectionsPhaseId = table.Column<string>(type: "text", nullable: false),
                    PageUrl = table.Column<string>(type: "text", nullable: true),
                    PhaseType = table.Column<int>(type: "integer", nullable: false),
                    SocialAccountId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitorForNewConnectionsPhases", x => x.MonitorForNewConnectionsPhaseId);
                    table.ForeignKey(
                        name: "FK_MonitorForNewConnectionsPhases_SocialAccounts_SocialAccount~",
                        column: x => x.SocialAccountId,
                        principalTable: "SocialAccounts",
                        principalColumn: "SocialAccountId");
                });

            migrationBuilder.CreateTable(
                name: "ScanProspectsForRepliesPhase",
                columns: table => new
                {
                    ScanProspectsForRepliesPhaseId = table.Column<string>(type: "text", nullable: false),
                    PhaseType = table.Column<int>(type: "integer", nullable: false),
                    PageUrl = table.Column<string>(type: "text", nullable: true),
                    SocialAccountId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanProspectsForRepliesPhase", x => x.ScanProspectsForRepliesPhaseId);
                    table.ForeignKey(
                        name: "FK_ScanProspectsForRepliesPhase_SocialAccounts_SocialAccountId",
                        column: x => x.SocialAccountId,
                        principalTable: "SocialAccounts",
                        principalColumn: "SocialAccountId");
                });

            migrationBuilder.CreateTable(
                name: "CampaignProspects",
                columns: table => new
                {
                    CampaignProspectId = table.Column<string>(type: "text", nullable: false),
                    PrimaryProspectId = table.Column<string>(type: "text", nullable: true),
                    CampaignProspectListId = table.Column<string>(type: "text", nullable: true),
                    ProfileUrl = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    AcceptedTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true),
                    FollowUpComplete = table.Column<bool>(type: "boolean", nullable: false),
                    ResponseMessage = table.Column<string>(type: "text", nullable: true),
                    LastFollowUpMessageSentTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    ConnectionSent = table.Column<bool>(type: "boolean", nullable: false),
                    ConnectionSentTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    Replied = table.Column<bool>(type: "boolean", nullable: false),
                    RepliedTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    SentFollowUpMessageOrderNum = table.Column<int>(type: "integer", nullable: false),
                    FollowUpMessageSent = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignProspects", x => x.CampaignProspectId);
                    table.ForeignKey(
                        name: "FK_CampaignProspects_CampaignProspectLists_CampaignProspectLis~",
                        column: x => x.CampaignProspectListId,
                        principalTable: "CampaignProspectLists",
                        principalColumn: "CampaignProspectListId");
                    table.ForeignKey(
                        name: "FK_CampaignProspects_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
                    table.ForeignKey(
                        name: "FK_CampaignProspects_PrimaryProspects_PrimaryProspectId",
                        column: x => x.PrimaryProspectId,
                        principalTable: "PrimaryProspects",
                        principalColumn: "PrimaryProspectId");
                });

            migrationBuilder.CreateTable(
                name: "CampaignWarmUps",
                columns: table => new
                {
                    CampaignWarmUpId = table.Column<string>(type: "text", nullable: false),
                    DailyLimit = table.Column<int>(type: "integer", nullable: false),
                    StartDateTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignWarmUps", x => x.CampaignWarmUpId);
                    table.ForeignKey(
                        name: "FK_CampaignWarmUps_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
                });

            migrationBuilder.CreateTable(
                name: "FollowUpMessages",
                columns: table => new
                {
                    FollowUpMessageId = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpMessages", x => x.FollowUpMessageId);
                    table.ForeignKey(
                        name: "FK_FollowUpMessages_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
                });

            migrationBuilder.CreateTable(
                name: "FollowUpMessagesPhases",
                columns: table => new
                {
                    FollowUpMessagePhaseId = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true),
                    PageUrl = table.Column<string>(type: "text", nullable: true),
                    PhaseType = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpMessagesPhases", x => x.FollowUpMessagePhaseId);
                    table.ForeignKey(
                        name: "FK_FollowUpMessagesPhases_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
                });

            migrationBuilder.CreateTable(
                name: "ProspectListPhases",
                columns: table => new
                {
                    ProspectListPhaseId = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true),
                    PhaseType = table.Column<int>(type: "integer", nullable: false),
                    SearchUrls = table.Column<List<string>>(type: "text[]", nullable: true),
                    Completed = table.Column<bool>(type: "boolean", nullable: false),
                    CompletedTimestamp = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProspectListPhases", x => x.ProspectListPhaseId);
                    table.ForeignKey(
                        name: "FK_ProspectListPhases_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
                });

            migrationBuilder.CreateTable(
                name: "SearchUrlsProgress",
                columns: table => new
                {
                    SearchUrlProgressId = table.Column<string>(type: "text", nullable: false),
                    WindowHandleId = table.Column<string>(type: "text", nullable: true),
                    LastPage = table.Column<int>(type: "integer", nullable: false),
                    Exhausted = table.Column<bool>(type: "boolean", nullable: false),
                    StartedCrawling = table.Column<bool>(type: "boolean", nullable: false),
                    LastActivityTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    TotalSearchResults = table.Column<int>(type: "integer", nullable: false),
                    LastProcessedProspect = table.Column<int>(type: "integer", nullable: false),
                    SearchUrl = table.Column<string>(type: "text", nullable: true),
                    CampaignId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchUrlsProgress", x => x.SearchUrlProgressId);
                    table.ForeignKey(
                        name: "FK_SearchUrlsProgress_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
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
                        principalColumn: "CampaignId");
                });

            migrationBuilder.CreateTable(
                name: "SendConnectionsStages",
                columns: table => new
                {
                    SendConnectionsStageId = table.Column<string>(type: "text", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true),
                    StartTime = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    NumOfConnections = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SendConnectionsStages", x => x.SendConnectionsStageId);
                    table.ForeignKey(
                        name: "FK_SendConnectionsStages_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
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

            migrationBuilder.CreateTable(
                name: "SentConnectionsStatuses",
                columns: table => new
                {
                    SearchUrlDetailsId = table.Column<string>(type: "text", nullable: false),
                    FinishedCrawling = table.Column<bool>(type: "boolean", nullable: false),
                    StartedCrawling = table.Column<bool>(type: "boolean", nullable: false),
                    WindowHandleId = table.Column<string>(type: "text", nullable: true),
                    OriginalUrl = table.Column<string>(type: "text", nullable: true),
                    CurrentUrl = table.Column<string>(type: "text", nullable: true),
                    LastActivityTimestamp = table.Column<long>(type: "bigint", nullable: false),
                    CampaignId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SentConnectionsStatuses", x => x.SearchUrlDetailsId);
                    table.ForeignKey(
                        name: "FK_SentConnectionsStatuses_Campaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "Campaigns",
                        principalColumn: "CampaignId");
                });

            migrationBuilder.CreateTable(
                name: "CampaignProspectFollowUpMessages",
                columns: table => new
                {
                    CampaignProspectFollowUpMessageId = table.Column<string>(type: "text", nullable: false),
                    CampaignProspectId = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampaignProspectFollowUpMessages", x => x.CampaignProspectFollowUpMessageId);
                    table.ForeignKey(
                        name: "FK_CampaignProspectFollowUpMessages_CampaignProspects_Campaign~",
                        column: x => x.CampaignProspectId,
                        principalTable: "CampaignProspects",
                        principalColumn: "CampaignProspectId");
                });

            migrationBuilder.CreateTable(
                name: "FollowUpMessageDelay",
                columns: table => new
                {
                    FollowUpMessageDelayId = table.Column<string>(type: "text", nullable: false),
                    FollowUpMessageId = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<long>(type: "bigint", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowUpMessageDelay", x => x.FollowUpMessageDelayId);
                    table.ForeignKey(
                        name: "FK_FollowUpMessageDelay_FollowUpMessages_FollowUpMessageId",
                        column: x => x.FollowUpMessageId,
                        principalTable: "FollowUpMessages",
                        principalColumn: "FollowUpMessageId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspectFollowUpMessages_CampaignProspectId",
                table: "CampaignProspectFollowUpMessages",
                column: "CampaignProspectId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspectLists_PrimaryProspectListId",
                table: "CampaignProspectLists",
                column: "PrimaryProspectListId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspects_CampaignId",
                table: "CampaignProspects",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspects_CampaignProspectListId",
                table: "CampaignProspects",
                column: "CampaignProspectListId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignProspects_PrimaryProspectId",
                table: "CampaignProspects",
                column: "PrimaryProspectId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ApplicationUserId",
                table: "Campaigns",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_CampaignProspectListId",
                table: "Campaigns",
                column: "CampaignProspectListId");

            migrationBuilder.CreateIndex(
                name: "IX_CampaignWarmUps_CampaignId",
                table: "CampaignWarmUps",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_CloudMapDiscoveryServices_EcsServiceId",
                table: "CloudMapDiscoveryServices",
                column: "EcsServiceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConnectionWithdrawPhases_SocialAccountId",
                table: "ConnectionWithdrawPhases",
                column: "SocialAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EcsServiceRegistries_EcsServiceId",
                table: "EcsServiceRegistries",
                column: "EcsServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_EcsTasks_EcsServiceId",
                table: "EcsTasks",
                column: "EcsServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpMessageDelay_FollowUpMessageId",
                table: "FollowUpMessageDelay",
                column: "FollowUpMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpMessages_CampaignId",
                table: "FollowUpMessages",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowUpMessagesPhases_CampaignId",
                table: "FollowUpMessagesPhases",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HalUnits_ApplicationUserId",
                table: "HalUnits",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonitorForNewConnectionsPhases_SocialAccountId",
                table: "MonitorForNewConnectionsPhases",
                column: "SocialAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrimaryProspectLists_UserId",
                table: "PrimaryProspectLists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrimaryProspects_PrimaryProspectListId",
                table: "PrimaryProspects",
                column: "PrimaryProspectListId");

            migrationBuilder.CreateIndex(
                name: "IX_ProspectListPhases_CampaignId",
                table: "ProspectListPhases",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanProspectsForRepliesPhase_SocialAccountId",
                table: "ScanProspectsForRepliesPhase",
                column: "SocialAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SearchUrls_CampaignProspectListId",
                table: "SearchUrls",
                column: "CampaignProspectListId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchUrls_PrimaryProspectListId",
                table: "SearchUrls",
                column: "PrimaryProspectListId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchUrlsProgress_CampaignId",
                table: "SearchUrlsProgress",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SendConnectionRequestPhases_CampaignId",
                table: "SendConnectionRequestPhases",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SendConnectionsStages_CampaignId",
                table: "SendConnectionsStages",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SendEmailInvitePhases_CampaignId",
                table: "SendEmailInvitePhases",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SentConnectionsStatuses_CampaignId",
                table: "SentConnectionsStatuses",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialAccounts_ApplicationUserId",
                table: "SocialAccounts",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialAccounts_HalUnitId",
                table: "SocialAccounts",
                column: "HalUnitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialAccounts_UserId",
                table: "SocialAccounts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialAccounts_VirtualAssistantId",
                table: "SocialAccounts",
                column: "VirtualAssistantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupportedTimeZones_TimeZoneId",
                table: "SupportedTimeZones",
                column: "TimeZoneId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Customer_StripeId",
                table: "Users",
                column: "Customer_StripeId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VirtualAssistants_ApplicationUserId",
                table: "VirtualAssistants",
                column: "ApplicationUserId");

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

            migrationBuilder.CreateIndex(
                name: "IX_VirtualAssistants_HalUnitId",
                table: "VirtualAssistants",
                column: "HalUnitId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CampaignProspectFollowUpMessages");

            migrationBuilder.DropTable(
                name: "CampaignWarmUps");

            migrationBuilder.DropTable(
                name: "ChromeProfileNames");

            migrationBuilder.DropTable(
                name: "ConnectionWithdrawPhases");

            migrationBuilder.DropTable(
                name: "EcsServiceRegistries");

            migrationBuilder.DropTable(
                name: "EcsTasks");

            migrationBuilder.DropTable(
                name: "FollowUpMessageDelay");

            migrationBuilder.DropTable(
                name: "FollowUpMessageJobs");

            migrationBuilder.DropTable(
                name: "FollowUpMessagesPhases");

            migrationBuilder.DropTable(
                name: "MonitorForNewConnectionsPhases");

            migrationBuilder.DropTable(
                name: "OrphanedCloudResources");

            migrationBuilder.DropTable(
                name: "ProspectListPhases");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "ScanProspectsForRepliesPhase");

            migrationBuilder.DropTable(
                name: "SearchUrls");

            migrationBuilder.DropTable(
                name: "SearchUrlsProgress");

            migrationBuilder.DropTable(
                name: "SendConnectionRequestPhases");

            migrationBuilder.DropTable(
                name: "SendConnectionsStages");

            migrationBuilder.DropTable(
                name: "SendEmailInvitePhases");

            migrationBuilder.DropTable(
                name: "SentConnectionsStatuses");

            migrationBuilder.DropTable(
                name: "SupportedTimeZones");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "CampaignProspects");

            migrationBuilder.DropTable(
                name: "FollowUpMessages");

            migrationBuilder.DropTable(
                name: "SocialAccounts");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "PrimaryProspects");

            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "VirtualAssistants");

            migrationBuilder.DropTable(
                name: "CampaignProspectLists");

            migrationBuilder.DropTable(
                name: "CloudMapDiscoveryServices");

            migrationBuilder.DropTable(
                name: "EcsTaskDefinitions");

            migrationBuilder.DropTable(
                name: "HalUnits");

            migrationBuilder.DropTable(
                name: "PrimaryProspectLists");

            migrationBuilder.DropTable(
                name: "EcsServices");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "StripeCustomers");
        }
    }
}
