using Hangfire;
using Leadsly.Application.Api.Configurations;
using Leadsly.Application.Api.Filters;
using Leadsly.Application.Api.Middlewares;
using Leadsly.Domain;
using Leadsly.Domain.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

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
            services.AddControllers()
                    .AddJsonOptionsConfiguration();

            services.AddDatabaseConnectionString(Configuration, Environment)
                    .AddConnectionProviders(Environment)
                    .AddJsonWebTokenConfiguration(Configuration)
                    .AddAuthorizationConfiguration()
                    .AddCorsConfiguration(Configuration)
                    .AddApiBehaviorOptionsConfiguration()
                    .AddSupervisorConfiguration()
                    .AddProvidersConfiguration()
                    .AddFacadesConfiguration()
                    .AddServicesConfiguration()
                    .AddLeadslyDependencies(Configuration)
                    .AddHangfireConfig()
                    .AddIdentityConfiguration(Configuration)
                    .AddHttpContextAccessor()
                    .AddEmailServiceConfiguration()
                    .AddRepositories()
                    .AddCommandHandlersConfiguration()
                    .AddFactories()
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

            if (env.IsDevelopment() || env.IsStaging())
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

            DashboardOptions options = new DashboardOptions()
            {
                Authorization = new[] { new HangfireAuthFilter() }
            };
            app.UseHangfireDashboard("/hangfire", options);

            app.SeedDatabase();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
