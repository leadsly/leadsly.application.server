using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Leadsly.Application.Model.Entities;
using Leadsly.Infrastructure.Configurations;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.Entities.Campaigns.Phases;

namespace Leadsly.Infrastructure
{
    public class DatabaseContext : IdentityDbContext<ApplicationUser>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options, ILogger<DatabaseContext> logger) : base(options) 
        {
            _logger = logger;
        }

        private ILogger<DatabaseContext> _logger;

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Customer_Stripe> StripeCustomers { get; set; }        
        public DbSet<EcsService> EcsServices { get; set; }
        public DbSet<EcsTaskDefinition> EcsTaskDefinitions { get; set; }
        public DbSet<CloudMapServiceDiscoveryService> CloudMapServiceDiscoveryServices { get; set; }
        public DbSet<EcsServiceRegistry> EcsServiceRegistries { get; set; }
        public DbSet<SocialAccount> SocialAccounts { get; set; } 
        public DbSet<SocialAccountCloudResource> SocialAccountResources { get; set; }
        public DbSet<OrphanedCloudResource> OrphanedCloudResources { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<ProspectListPhase> ProspectListPhases { get; set; }
        public DbSet<MonitorForNewConnectionsPhase> MonitorForNewConnectionsPhases { get; set; }
        public DbSet<FollowUpMessagePhase> FollowUpMessagesPhases { get; set; }
        public DbSet<FollowUpMessage> FollowUpMessages { get; set; }
        public DbSet<CampaignProspectFollowUpMessage> CampaignProspectFollowUpMessages { get; set; }
        public DbSet<ScanProspectsForRepliesPhase> ScanProspectsForRepliesPhase { get; set; }
        public DbSet<ConnectionWithdrawPhase> ConnectionWithdrawPhases { get; set; }
        public DbSet<SendConnectionRequestPhase> SendConnectionRequestPhases { get; set; }
        public DbSet<SendEmailInvitePhase> SendEmailInvitePhases { get; set; }        
        public DbSet<PrimaryProspectList> PrimaryProspectLists { get; set; }
        public DbSet<CampaignProspectList> CampaignProspectLists { get; set; }
        public DbSet<PrimaryProspect> PrimaryProspects { get; set; }
        public DbSet<CampaignProspect> CampaignProspects { get; set; }
        public DbSet<HalUnit> HalUnits { get; set; }
        public DbSet<SearchUrl> SearchUrls { get; set; }
        public DbSet<SearchUrlDetails> SentConnectionsStatuses { get; set; }
        public DbSet<ChromeProfile> ChromeProfileNames { get; set; }
        public DbSet<CampaignWarmUp> CampaignWarmUps { get; set; }
        public DbSet<SendConnectionsStage> SendConnectionsStages { get; set; }
        public DbSet<SearchUrlProgress> SearchUrlsProgress { get; set; }
        public DbSet<FollowUpMessageJob> FollowUpMessageJobs { get; set; }
        public DbSet<LeadslyTimeZone> SupportedTimeZones { get; set; }
        public DbSet<HalTimeZone> TimeZonesHals { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            this._logger.LogInformation("Configuring custom database settings.");

            IdentityUsersConfiguration.ConfigureIdentityUsersTableNames(builder, _logger);

            StripeCustomersConfiguration.Configure(builder, _logger);

            SocialAccountsConfiguration.Configure(builder, _logger);
        }
    }
}
