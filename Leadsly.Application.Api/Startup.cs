using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using Leadsly.Domain;
using Leadsly.Api.Middlewares;
using Leadsly.Application.Api.Configurations;
using Hangfire;
using Leadsly.Domain.OptionsJsonModels;
using Amazon.RDS.Util;
using Leadsly.Application.Model;
using Leadsly.Domain.Supervisor;
using Leadsly.Domain.Services;

namespace Leadsly.Application.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            PostgresOptions postgresOptions = new();
            Configuration.GetSection(nameof(PostgresOptions)).Bind(postgresOptions);
            string authToken = RDSAuthTokenGenerator.GenerateAuthToken(postgresOptions.Host, postgresOptions.Port, postgresOptions.UserId);
            string defaultConnection = $"Host={postgresOptions.Host};User Id={postgresOptions.UserId};Password={authToken};Database={postgresOptions.Database}";

            services.AddControllers()
                    .AddJsonOptionsConfiguration();

            services.AddConnectionProviders(Configuration, Environment, defaultConnection)
                    .AddJsonWebTokenConfiguration(Configuration)
                    .AddAuthorizationConfiguration()
                    .AddCorsConfiguration(Configuration)
                    .AddApiBehaviorOptionsConfiguration()
                    .AddSupervisorConfiguration()
                    .AddLeadslyProviders()
                    .AddFacades()
                    .AddLeadslyServices()
                    .AddLeadslyDependencies(Configuration)
                    .AddHangfireConfig(defaultConnection)
                    .AddIdentityConfiguration(Configuration)
                    .AddHttpContextAccessor()
                    .AddEmailServiceConfiguration()
                    .AddRepositories()
                    .AddRabbitMQConfiguration(Configuration)
                    .AddServices(Configuration)
                    .AddRemoveNull204FormatterConfigration()
                    .AddMemoryCache()
                    .AddHostedService<ProducingHostedService>();

            services.Configure<MvcOptions>(ApiDefaults.Configure);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if request does not contain api it will also work
            app.UsePathBase("/api");

            if (env.IsDevelopment())
            {
                app.UseCors(ApiConstants.Cors.AllowAll);
            }
            else
            {
                app.UseCors(ApiConstants.Cors.WithOrigins);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSerilogRequestLogging();

            app.UseHangfireDashboard("/hangfire-leadsly-server");

            app.SeedDatabase();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });            
        }
    }
}
