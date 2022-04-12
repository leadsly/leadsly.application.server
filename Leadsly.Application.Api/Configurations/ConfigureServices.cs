using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.EntityFrameworkCore;
using Leadsly.Domain.DbInfo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Leadsly.Domain.Supervisor;
using Newtonsoft.Json.Converters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using System.Linq;
using Serilog;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Stripe;
using Leadsly.Domain.Services;
using Leadsly.Application.Model.Entities;
using Leadsly.Infrastructure.Repositories;
using Leadsly.Domain.Repositories;
using Leadsly.Domain;
using Leadsly.Infrastructure;
using Leadsly.Application.Api.DataProtectorTokenProviders;
using Leadsly.Application.Api.Services;
using Leadsly.Application.Api.Authentication;
using Leadsly.Domain.Providers;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;
using Amazon.ECS;
using Amazon;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Application.Domain.OptionsJsonModels;
using Amazon.ServiceDiscovery;
using Amazon.Route53;
using Hangfire;
using Hangfire.PostgreSql;
using Leadsly.Domain.Services.Interfaces;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Facades;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Serializers.Interfaces;
using Leadsly.Domain.Serializers;
using Leadsly.Domain.Campaigns;

namespace Leadsly.Application.Api.Configurations
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddConnectionProviders(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, string defaultConnection)
        {
            Log.Information("Configuring default connection string and database context.");

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(defaultConnection);
                if (env.IsDevelopment())
                {
                    Log.Information("Enabling SQL sensitive data logging.");
                    options.EnableSensitiveDataLogging(env.IsDevelopment());
                    options.EnableDetailedErrors(env.IsDevelopment());                    
                }
            }, ServiceLifetime.Scoped);

            services.AddSingleton(new DbInfo(defaultConnection));

            return services;
        }

        public static IServiceCollection AddSupervisorConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering supervisor services.");

            services.AddScoped<ISupervisor, Supervisor>();            

            return services;
        }

        public static IServiceCollection AddRabbitMQConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering rabbit mq services configuration.");

            services.Configure<RabbitMQConfigOptions>(options => configuration.GetSection(nameof(RabbitMQConfigOptions)).Bind(options));

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            Log.Information("Registering repository services.");

            services.AddScoped<IStripeRepository, StripeRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICloudPlatformRepository, CloudPlatformRepository>();
            services.AddScoped<ISocialAccountRepository, SocialAccountRepository>();
            services.AddScoped<IOrphanedCloudResourcesRepository, OrphanedCloudResourcesRepository>();
            services.AddScoped<ICampaignRepository, CampaignRepository>();
            services.AddScoped<IRabbitMQRepository, RabbitMQRepository>();            
            services.AddScoped<IHalRepository, HalRepository>();
            services.AddScoped<IProspectListPhaseRepository, ProspectListPhaseRepository>();
            services.AddScoped<IMonitorForNewConnectionsPhaseRepository, MonitorForNewConnectionsPhaseRepository>();
            services.AddScoped<IScanProspectsForRepliesPhaseRepository, ScanProspectsForRepliesPhaseRepository>();
            services.AddScoped<IConnectionWithdrawPhaseRepository, ConnectionWithdrawPhaseRepository>();
            services.AddScoped<IPrimaryProspectRepository, PrimaryProspectRepository>();
            services.AddScoped<ICampaignProspectRepository, CampaignProspectRepository>();
            services.AddScoped<ISendConnectionsPhaseRepository, SendConnectionsPhaseRepository>();
            services.AddScoped<IFollowUpMessagePhaseRepository, FollowUpMessagePhaseRepository>();

            return services;
        }

        public static IServiceCollection AddHangfireConfig(this IServiceCollection services, string defaultConnection)
        {
            Log.Information("Registering hangfire services.");

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

            services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(defaultConnection);
                config.UseRecommendedSerializerSettings();
            }).AddHangfireServer();

            return services;
        }

        public static IServiceCollection AddLeadslyDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering leadsly dependencies.");

            //LeadslyBotApiOptions options = new LeadslyBotApiOptions();
            //configuration.GetSection(nameof(LeadslyBotApiOptions)).Bind(options);

            services.AddHttpClient<ILeadslyHalApiService, LeadslyHalApiService>();

            services.Configure<CloudPlatformConfigurationOptions>(options => configuration.GetSection(nameof(CloudPlatformConfigurationOptions)).Bind(options));
            CloudPlatformConfigurationOptions cloudPlatformConfigurationOptions = new CloudPlatformConfigurationOptions();
            configuration.GetSection(nameof(CloudPlatformConfigurationOptions)).Bind(cloudPlatformConfigurationOptions);
            AWSConfigs.AWSRegion = cloudPlatformConfigurationOptions.AwsOptions.Region;

            services.AddScoped(typeof(AmazonECSClient));
            services.AddScoped(typeof(AmazonServiceDiscoveryClient));
            services.AddScoped(typeof(AmazonRoute53Client));            
            services.AddScoped<IConnectAccountResponseSerializer, ConnectAccountResponseSerializer>();
            services.AddScoped<IEnterTwoFactorAuthCodeResponseSerializer, EnterTwoFactorAuthCodeResponseSerializer>();
            services.AddScoped<ICampaignPhaseSerializer, CampaignPhaseSerializer>();
            
            return services;
        }
        public static IServiceCollection AddProvidersConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering leadsly providers.");

            services.AddScoped<ICloudPlatformProvider, CloudPlatformProvider>();
            services.AddScoped<IUserProvider, UserProvider>();
            services.AddScoped<ILeadslyHalProvider, LeadslyHalProvider>();
            services.AddScoped<IMonitorNewConnectionsProvider, MonitorNewConnectionsProvider>();
            services.AddScoped<ICampaignProvider, CampaignProvider>();
            services.AddScoped<IRabbitMQProvider, RabbitMQProvider>();

            return services;
        }

        public static IServiceCollection AddFacadesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering facades services.");

            services.AddScoped<ISerializerFacade, SerializerFacade>();
            services.AddScoped<ICampaignRepositoryFacade, CampaignRepositoryFacade>();
            
            return services;
        }

        public static IServiceCollection AddServicesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering leadsly services.");

            services.AddScoped<IAwsElasticContainerService, AwsElasticContainerService>();
            services.AddScoped<IAwsServiceDiscoveryService, AwsServiceDiscoveryService>();
            services.AddScoped<IAwsRoute53Service, AwsRoute53Service>();
            services.AddScoped<ILeadslyHalApiService, LeadslyHalApiService>();
            services.AddSingleton<IProducingService, ProducingService>();
            services.AddScoped<ICampaignService, CampaignService>();
            
            services.AddSingleton<ICampaignManager, CampaignManager>();
            services.AddSingleton<ICampaignPhaseProducer, CampaignPhaseProducer>();

            return services;
        }

        public static IServiceCollection AddJsonWebTokenConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring Jwt services.");

            IConfigurationSection jwtAppSettingOptions = configuration.GetSection(nameof(JwtIssuerOptions));

            // retrieve private key from user secrets or azure vault
            string privateKey = configuration[ApiConstants.VaultKeys.JwtSecret];
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(privateKey));

            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);
            });

            TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(configureOptions =>
            {
                configureOptions.ClaimsIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                configureOptions.TokenValidationParameters = tokenValidationParameters;
                configureOptions.SaveToken = true;
                // In case of having an expired token
                configureOptions.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add(ApiConstants.TokenOptions.ExpiredToken, "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
        
        public static IServiceCollection AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Adding identity services.");

            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 3;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 10;

                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = true;
            })
            .AddDefaultTokenProviders()
            .AddTokenProvider<StaySignedInDataProtectorTokenProvider<ApplicationUser>>(ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName)
            .AddTokenProvider<FacebookDataProtectorTokenProvider<ApplicationUser>>(ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook)
            .AddTokenProvider<GoogleDataProtectorTokenProvider<ApplicationUser>>(ApiConstants.DataTokenProviders.ExternalLoginProviders.Google)
            .AddTokenProvider<SignUpDataProtectorTokenProvider<ApplicationUser>>(ApiConstants.DataTokenProviders.RegisterNewUserProvider.ProviderName)
            .AddTokenProvider<EmailConfirmationDataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultEmailProvider)
            .AddRoles<IdentityRole>()            
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddSignInManager()
            .AddUserManager<LeadslyUserManager>()
            .AddEntityFrameworkStores<DatabaseContext>(); // Tell identity which EF DbContext to use;

            // email token provider settings
            services.Configure<EmailConfirmationDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = TokenOptions.DefaultEmailProvider;
                options.TokenLifespan = TimeSpan.FromDays(3);
            });

            // stay signed in provider settings
            services.Configure<StaySignedInDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = ApiConstants.DataTokenProviders.StaySignedInProvider.ProviderName;
                options.TokenLifespan = TimeSpan.FromDays(7);
            });

            // sign up new users provider settings
            services.Configure<SignUpDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = ApiConstants.DataTokenProviders.RegisterNewUserProvider.ProviderName;

                IConfigurationSection registrationSection = configuration.GetSection(nameof(UserRegistrationOptions));
                IConfigurationSection token = registrationSection.GetSection(nameof(UserRegistrationOptions.Token));
                string tokenLifeSpan = token[nameof(UserRegistrationOptions.Token.LifeSpanInDays)] ?? throw new ArgumentNullException("You must provide user registration token value.");
                double tokenSpan = Convert.ToDouble(tokenLifeSpan);
                options.TokenLifespan = TimeSpan.FromDays(tokenSpan);
            });

            // facebook token provider settings
            services.Configure<FacebookDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = ApiConstants.DataTokenProviders.ExternalLoginProviders.Facebook;
                options.TokenLifespan = TimeSpan.FromDays(120);
                options.ClientId = configuration[ApiConstants.VaultKeys.FaceBookClientId];
                options.ClientSecret = configuration[ApiConstants.VaultKeys.FaceBookClientSecret];
            });

            // google token provider settings
            services.Configure<GoogleDataProtectionTokenProviderOptions>(options =>
            {
                options.Name = ApiConstants.DataTokenProviders.ExternalLoginProviders.Google;
                options.TokenLifespan = TimeSpan.FromDays(120);
                options.ClientId = configuration[ApiConstants.VaultKeys.GoogleClientId];
                options.ClientSecret = configuration[ApiConstants.VaultKeys.GoogleClientSecret];
            });

            //Configure Claims Identity
            services.AddScoped<IClaimsIdentityService, ClaimsIdentityService>();

            return services;
        }

        public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
        {
            Log.Information("Configuring authorization options.");

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                                .AddRequirements(new DenyAnonymousAuthorizationRequirement())
                                .Build();
            });

            return services;
        }

        public static IServiceCollection AddRemoveNull204FormatterConfigration(this IServiceCollection services)
        {
            Log.Information("Configuring output formatters.");

            services.AddControllers(opt =>
            {
                // remove formatter that turns nulls into 204 - Angular http client treats 204s as failed requests
                HttpNoContentOutputFormatter noContentFormatter = opt.OutputFormatters.OfType<HttpNoContentOutputFormatter>().FirstOrDefault();
                if(noContentFormatter != null)
                {
                    noContentFormatter.TreatNullValueAsNoContent = false;
                }
            });

            return services;
        }

        public static IMvcBuilder AddJsonOptionsConfiguration(this IMvcBuilder builder)
        {
            Log.Information("Configuring NewtonsoftJson options.");

            builder.AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                // Serialize the name of enum values rather than their integer value
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            return builder;
        }

        public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring Cors.");

            services.AddCors(options =>
            {
                options.AddPolicy(ApiConstants.Cors.WithOrigins, new CorsPolicyBuilder()
                                                      .WithOrigins(configuration["AllowedOrigins"])
                                                      .AllowAnyHeader()
                                                      .AllowCredentials()
                                                      .AllowAnyMethod()
                                                      .Build());


                options.AddPolicy(ApiConstants.Cors.AllowAll, new CorsPolicyBuilder()
                                                     .AllowAnyOrigin()
                                                     .AllowAnyHeader()
                                                     .AllowAnyMethod()
                                                     .Build());
            });

            return services;
        }

        public static IServiceCollection AddApiBehaviorOptionsConfiguration(this IServiceCollection services)
        {
            Log.Information("Configuring ApiBehaviorOptions.");

            // Required to surpress automatic problem details returned by asp.net core framework when ModelState.IsValid == false.
            // Allows for custom IActionFilter implementation and response. See InvalidModelStateFilter.
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;
        }

        public static IServiceCollection AddEmailServiceConfiguration(this IServiceCollection services)
        {
            Log.Information("Configuring AddEmailServiceConfiguration.");

            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IHtmlTemplateGenerator, HtmlTemplateGenerator>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Configuring AddServices.");

            Log.Information("Configuring Stripe.");
            StripeConfiguration.ApiKey = configuration[nameof(ApiConstants.VaultKeys.StripeSecretKey)] ?? throw new ArgumentNullException("You must provide Stripe Secret Key.");
            services.AddScoped(typeof(CustomerService));           

            return services;
        }

    }
}
