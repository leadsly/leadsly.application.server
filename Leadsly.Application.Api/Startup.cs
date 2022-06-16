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
using Leadsly.Application.Api.Configurations;
using Hangfire;
using Leadsly.Domain.OptionsJsonModels;
using Amazon.RDS.Util;
using Leadsly.Application.Model;
using Leadsly.Domain.Supervisor;
using Leadsly.Domain.Services;
using Leadsly.Application.Api.Middlewares;
using Leadsly.Application.Api.Filters;

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
                    .AddHostedService<ProducingHostedService>()
                    .AddSpaStaticFiles(spa =>
                    {
                        spa.RootPath = "wwwroot";
                    });

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

            app.UseDefaultFiles();

            app.UseSpaStaticFiles();

            //app.UseSpaStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "fonts")),
            //    RequestPath = "/wwwroot/assets/fonts",
            //    OnPrepareResponse = ctx =>
            //    {
            //        ctx.Context.Response.Headers.Append("Cache-Control", "private, max-age=86400, stale-while-revalidate=604800");
            //    }
            //});

            //app.UseSpaStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "iconfont")),
            //    RequestPath = "/wwwroot/assets/iconfont",
            //    OnPrepareResponse = ctx =>
            //    {
            //        ctx.Context.Response.Headers.Append("Cache-Control", "private, max-age=86400, stale-while-revalidate=604800");
            //    }
            //});

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
