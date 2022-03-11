using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Leadsly.Application.Model.Entities;
using Leadsly.Infrastructure.Configurations;
using Leadsly.Application.Model.Entities.Campaigns;

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
