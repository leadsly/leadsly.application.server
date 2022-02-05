﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Leadsly.Models.Entities;
using Leadsly.Infrastructure.Configurations;

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
        public DbSet<DockerContainerInfo> DockerContainers { get; set; }
        public DbSet<ECSService> ECSServices { get; set; }
        public DbSet<ECSTask> ECSTasks { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<SocialAccount> SocialAccounts { get; set; }        
        public DbSet<Campaign> Campaigns { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            this._logger.LogInformation("Configuring custom database settings.");

            IdentityUsersConfiguration.ConfigureIdentityUsersTableNames(builder, _logger);

            StripeCustomersConfiguration.Configure(builder, _logger);
        }
    }
}
