using Amazon;
using Amazon.ECS;
using Amazon.Extensions.NETCore.Setup;
using Amazon.RDS.Util;
using Amazon.Route53;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.ServiceDiscovery;
using Hangfire;
using Hangfire.PostgreSql;
using Leadsly.Application.Api.Authentication;
using Leadsly.Application.Api.DataProtectorTokenProviders;
using Leadsly.Application.Api.Services;
using Leadsly.Domain;
using Leadsly.Domain.DbInfo;
using Leadsly.Domain.Decorators;
using Leadsly.Domain.Facades;
using Leadsly.Domain.Facades.Interfaces;
using Leadsly.Domain.Factories;
using Leadsly.Domain.Factories.Interfaces;
using Leadsly.Domain.JobServices;
using Leadsly.Domain.JobServices.Interfaces;
using Leadsly.Domain.Models.Entities;
using Leadsly.Domain.MQ;
using Leadsly.Domain.MQ.Creators;
using Leadsly.Domain.MQ.Creators.Interfaces;
using Leadsly.Domain.MQ.EventHandlers;
using Leadsly.Domain.MQ.EventHandlers.Interfaces;
using Leadsly.Domain.MQ.Interfaces;
using Leadsly.Domain.MQ.Services;
using Leadsly.Domain.MQ.Services.Interfaces;
using Leadsly.Domain.OptionsJsonModels;
using Leadsly.Domain.PhaseConsumers;
using Leadsly.Domain.PhaseConsumers.TriggerFollowUpMessagesHandler;
using Leadsly.Domain.PhaseConsumers.TriggerScanProspectsForRpliesHandlers;
using Leadsly.Domain.PhaseHandlers;
using Leadsly.Domain.PhaseHandlers.TriggerFollowUpMessagesHandler;
using Leadsly.Domain.PhaseHandlers.TriggerScanProspectsForRepliesHandler;
using Leadsly.Domain.Providers;
using Leadsly.Domain.Providers.Interfaces;
using Leadsly.Domain.Repositories;
using Leadsly.Domain.Services;
using Leadsly.Domain.Services.Interfaces;
using Leadsly.Domain.Supervisor;
using Leadsly.Infrastructure;
using Leadsly.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;
using Stripe;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Api.Configurations
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddConnectionProviders(this IServiceCollection services, IWebHostEnvironment env)
        {
            Log.Information("Configuring default connection string and database context.");

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IDbInfo dbInfo = serviceProvider.GetRequiredService<IDbInfo>();

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(dbInfo.ConnectionString);
                if (env.IsDevelopment())
                {
                    Log.Information("Enabling SQL sensitive data logging.");
                    options.EnableSensitiveDataLogging(env.IsDevelopment());
                    options.EnableDetailedErrors(env.IsDevelopment());
                }
            }, ServiceLifetime.Scoped);

            return services;
        }

        public static IServiceCollection AddDatabaseConnectionString(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env, bool useIAMAuth = false)
        {
            DatabaseConnections databaseConnections = new();
            configuration.GetSection(nameof(DatabaseConnections)).Bind(databaseConnections);
            string defaultConnection = string.Empty;
            if (useIAMAuth)
            {
                // IAM Authentication code. Token valid only for 15 mins and needs to be renewed after that                
                string authToken = RDSAuthTokenGenerator.GenerateAuthToken(databaseConnections.IAMAuth.Host, databaseConnections.IAMAuth.Port, databaseConnections.IAMAuth.UserId);
                defaultConnection = $"Host={databaseConnections.IAMAuth.Host};User Id={databaseConnections.IAMAuth.UserId};Password={authToken};Database={databaseConnections.Database};Include Error Detail=true";
            }
            else
            {
                if (env.IsDevelopment())
                {
                    // secrets.json
                    string userName = configuration[$"{databaseConnections.AuthCredentials.Key}:Username"];
                    string host = configuration[$"{databaseConnections.AuthCredentials.Key}:Host"];
                    string password = configuration[$"{databaseConnections.AuthCredentials.Key}:Password"];
                    defaultConnection = $"Host={host};User Id={userName};Password={password};Database={databaseConnections.Database};Include Error Detail=true";
                }
                else
                {
                    DatabaseConnectionInformation dbConnectionInfo = AwsSecretsFetcher.GetSecret<DatabaseConnectionInformation>(databaseConnections.AuthCredentials.Key, databaseConnections.AuthCredentials.AwsRegion);
                    defaultConnection = $"Host={dbConnectionInfo.Host};User Id={dbConnectionInfo.UserName};Password={dbConnectionInfo.Password};Database={databaseConnections.Database};Include Error Detail=true";
                }
            }

            services.AddSingleton(options =>
            {
                IDbInfo dbInfo = new DbInfo(defaultConnection);
                return dbInfo;
            });

            return services;
        }

        public static IServiceCollection AddSupervisorConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering JsonOptions classes.");

            services.Configure<S3BucketOptions>(options => configuration.GetSection(nameof(S3BucketOptions)).Bind(options));

            return services;
        }

        public static IServiceCollection AddSupervisorConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering supervisor services.");

            services.AddScoped<ISupervisor, Supervisor>();

            return services;
        }

        public static IServiceCollection AddJobServicesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering job services used for scheduled jobs.");

            services.AddScoped<ICampaignJobsService, CampaignJobsService>();
            services.AddScoped<ICheckOffHoursNewConnectionsJobService, CheckOffHoursNewConnectionsJobService>();
            services.AddScoped<IMonitorForNewConnectionsJobService, MonitorForNewConnectionsJobService>();
            services.AddScoped<INetworkingJobsService, NetworkingJobsService>();
            services.AddScoped<INewTimeZonesJobsService, NewTimeZonesJobsService>();
            services.AddScoped<IProspectingJobService, ProspectingJobService>();
            services.AddScoped<IRestartHalJobService, RestartHalJobService>();
            services.AddScoped<IRestartHalsByTimezoneJobService, RestartHalsByTimezoneJobService>();

            return services;
        }

        public static IServiceCollection AddDecoratorsConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering decorators configuration");

            services.AddScoped<SaveBrowserProfileUserProviderDecorator>();

            return services;
        }

        public static IServiceCollection AddRabbitMQConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering rabbit mq services configuration.");

            services.Configure<RabbitMQConfigOptions>(options => configuration.GetSection(nameof(RabbitMQConfigOptions)).Bind(options));
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton(s =>
            {
                ObjectPoolProvider provider = s.GetRequiredService<ObjectPoolProvider>();
                IOptions<RabbitMQConfigOptions> rabbitMQConfigOptions = s.GetRequiredService<IOptions<RabbitMQConfigOptions>>();
                return provider.Create(new RabbitModelPooledObjectPolicy(rabbitMQConfigOptions.Value));
            });

            services.AddScoped<ICheckOffHoursNewConnectionsMQCreator, CheckOffHoursNewConnectionsMQCreator>();
            services.AddScoped<IDeepScanProspectsForRepliesMQCreator, DeepScanProspectsForRepliesMQCreator>();
            services.AddScoped<IFollowUpMessagesMQCreator, FollowUpMessagesMQCreator>();
            services.AddScoped<IMonitorForNewConnectionsMQCreator, MonitorForNewConnectionsMQCreator>();
            services.AddScoped<INetworkingMQCreator, NetworkingMQCreator>();
            services.AddScoped<IScanProspectsForRepliesMQCreator, ScanProspectsForRepliesMQCreator>();

            services.AddScoped<IMessageBrokerOutlet, MessageBrokerOutlet>();
            services.AddScoped<IRabbitMQManager, RabbitMQManager>();

            services.AddScoped<ICheckOffHoursNewConnectionsCreateMQService, CheckOffHoursNewConnectionsCreateMQService>();
            services.AddScoped<IDeepScanProspectsForRepliesCreateMQService, DeepScanProspectsForRepliesCreateMQService>();
            services.AddScoped<IFollowUpMessageCreateMQService, FollowUpMessageCreateMQService>();
            services.AddScoped<IFollowUpMessageMQPublisher, FollowUpMessageMQPublisher>();
            services.AddScoped<IFollowUpMessageMQPublisherService, FollowUpMessageMQPublisherService>();
            services.AddScoped<IFollowUpMessagesCreateMQService, FollowUpMessagesCreateMQService>();
            services.AddScoped<IFollowUpMessagesMQService, FollowUpMessagesMQService>();
            services.AddScoped<IMonitorForNewConnectionsCreateMQService, MonitorForNewConnectionsCreateMQService>();
            services.AddScoped<INetworkingCreateMQService, NetworkingCreateMQService>();
            services.AddScoped<IScanProspectsForRepliesCreateMQService, ScanProspectsForRepliesCreateMQService>();


            return services;
        }

        public static IServiceCollection AddFactoriesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering factories services.");

            services.AddScoped<INetworkingMessagesFactory, NetworkingMessagesFactory>();
            services.AddScoped<IScanProspectsForRepliesMessagesFactory, ScanProspectsForRepliesMessagesFactory>();
            services.AddScoped<IMonitorForNewConnectionsMessagesFactory, MonitorForNewConnectionsMessagesFactory>();
            services.AddScoped<IFollowUpMessagesFactory, FollowUpMessagesFactory>();
            services.AddScoped<ICheckOffHoursNewConnectionsFactory, CheckOffHoursNewConnectionsFactory>();
            services.AddScoped<IDeepScanProspectsForRepliesMessageFactory, DeepScanProspectsForRepliesMessageFactory>();
            services.AddScoped<IPersistBrowserProfileMessageFactory, PersistBrowserProfileMessageFactory>();
            services.AddScoped<IJwtFactory, JwtFactory>();

            return services;
        }

        public static IServiceCollection AddRepositoriesConfiguration(this IServiceCollection services)
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
            services.AddScoped<IPrimaryProspectRepository, PrimaryProspectRepository>();
            services.AddScoped<ICampaignProspectRepository, CampaignProspectRepository>();
            services.AddScoped<IFollowUpMessagePhaseRepository, FollowUpMessagePhaseRepository>();
            services.AddScoped<IFollowUpMessageRepository, FollowUpMessageRepository>();
            services.AddScoped<ISearchUrlProgressRepository, SearchUrlProgressRepository>();
            services.AddScoped<IFollowUpMessageJobsRepository, FollowUpMessageJobsRepository>();
            services.AddScoped<ITimeZoneRepository, TimeZoneRepository>();
            services.AddScoped<ISendConnectionsPhaseRepository, SendConnectionsPhaseRepository>();
            services.AddScoped<IVirtualAssistantRepository, VirtualAssistantRepository>();

            return services;
        }

        public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering hangfire services.");

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            IDbInfo dbInfo = serviceProvider.GetRequiredService<IDbInfo>();

            services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(dbInfo.ConnectionString);
                config.UseRecommendedSerializerSettings();
            }).AddHangfireServer();

            services.AddSingleton<IHangfireService, HangfireService>();

            return services;
        }

        public static IServiceCollection AddLeadslyDependenciesConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering leadsly dependencies.");

            services.AddHttpClient<ILeadslyHalApiService, LeadslyHalApiService>();

            services.Configure<CloudPlatformConfigurationOptions>(options => configuration.GetSection(nameof(CloudPlatformConfigurationOptions)).Bind(options));
            CloudPlatformConfigurationOptions cloudPlatformConfigurationOptions = new CloudPlatformConfigurationOptions();
            configuration.GetSection(nameof(CloudPlatformConfigurationOptions)).Bind(cloudPlatformConfigurationOptions);
            AWSConfigs.AWSRegion = cloudPlatformConfigurationOptions.AwsOptions.Region;

            services.AddScoped(opt =>
            {
                AmazonECSConfig config = new AmazonECSConfig
                {
                    Timeout = TimeSpan.FromSeconds(240),
                    ThrottleRetries = false
                };

                AmazonECSClient client = new AmazonECSClient(config);
                return client;
            });
            services.AddScoped(typeof(AmazonECSClient));
            services.AddScoped(typeof(AmazonServiceDiscoveryClient));
            services.AddScoped(typeof(AmazonRoute53Client));

            return services;
        }
        public static IServiceCollection AddProvidersConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering leadsly providers.");

            services.AddScoped<ICloudPlatformProvider, CloudPlatformProvider>();
            services.AddScoped<IUserProvider, UserProvider>();
            services.AddScoped<ILeadslyHalProvider, LeadslyHalProvider>();
            services.AddScoped<ICampaignProvider, CampaignProvider>();
            services.AddScoped<IRabbitMQProvider, RabbitMQProvider>();
            services.AddScoped<ISendFollowUpMessageProvider, SendFollowUpMessageProvider>();

            return services;
        }

        public static IServiceCollection AddFacadesConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering facades services.");

            services.AddScoped<ICampaignRepositoryFacade, CampaignRepositoryFacade>();
            services.AddScoped<IMQCreatorFacade, MQCreatorFacade>();

            return services;
        }

        public static IServiceCollection AddCommandHandlersConfiguration(this IServiceCollection services)
        {
            Log.Information("Registering command handlers.");
            services.AddScoped<ICommandHandler<TriggerFollowUpMessagesCommand>, TriggerFollowUpMessagesCommandHandler>();
            services.AddScoped<ICommandHandler<TriggerScanProspectsForRepliesCommand>, TriggerScanProspectsForRepliesCommandHandler>();

            // consumers for FollowUpMessages
            services.AddScoped<IConsumeCommandHandler<TriggerFollowUpMessagesConsumeCommand>, TriggerFollowUpMessagesConsumeCommandHandler>();
            services.AddScoped<ITriggerFollowUpMessagesEventHandler, TriggerFollowUpMessagesEventHandler>();

            // consumers for ScanProspectsForReplies
            services.AddScoped<IConsumeCommandHandler<TriggerScanProspectsForRepliesConsumeCommand>, TriggerScanProspectsForRepliesConsumeCommandHandler>();
            services.AddScoped<ITriggerScanProspectsForRepliesEventHandler, TriggerScanProspectsForRepliesEventHandler>();

            return services;
        }

        public static IServiceCollection AddServicesConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Information("Registering leadsly services.");

            services.AddSingleton<IProducingService, ProducingService>();
            services.AddSingleton<IConsumingService, ConsumingService>();
            services.AddScoped<IAwsElasticContainerService, AwsElasticContainerService>();
            services.AddScoped<IAwsServiceDiscoveryService, AwsServiceDiscoveryService>();
            services.AddScoped<IAwsRoute53Service, AwsRoute53Service>();
            services.AddScoped<IAwsS3Service, AwsS3Service>();
            AWSOptions awsOptions = configuration.GetAWSOptions();
            awsOptions.Credentials = new EnvironmentVariablesAWSCredentials();
            services.AddDefaultAWSOptions(awsOptions);
            services.AddSingleton<IAmazonS3>(opt =>
            {
                IAmazonS3 client = awsOptions.CreateServiceClient<IAmazonS3>();
                return client;
            });

            services.AddScoped<ILeadslyHalApiService, LeadslyHalApiService>();
            services.AddScoped<ITimestampService, TimestampService>();
            services.AddScoped<IUrlService, UrlService>();
            services.AddScoped<IAccessTokenService, AccessTokenService>();
            services.AddScoped<ICreateCampaignService, CreateCampaignService>();
            services.AddScoped<IPreventFollowUpMessageService, PreventFollowUpMessageService>();
            services.AddScoped<IRestartHalService, RestartHalService>();

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
                ValidateIssuer = false,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = false,
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
                if (noContentFormatter != null)
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
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            });

            //JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            //{
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //    Converters = new[] { new StringEnumConverter() },
            //    NullValueHandling = NullValueHandling.Ignore,
            //    MissingMemberHandling = MissingMemberHandling.Ignore
            //};

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
