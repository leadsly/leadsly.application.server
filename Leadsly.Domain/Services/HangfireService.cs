using Hangfire;
using Hangfire.Annotations;
using Leadsly.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services
{
    public class HangfireService : IHangfireService
    {
        public const string OnboardNewUnits_RecurringJobId = "OnboardNewHalUnits";        
        public static string ActiveCampaignsCronSchedule = Cron.Daily(6, 40);        

        public HangfireService(ILogger<HangfireService> logger)
        {            
            _logger = logger;
            ServerTimeZone = TimeZoneInfo.FindSystemTimeZoneById(ServerTimeZoneId);
        }

        public TimeZoneInfo ServerTimeZone { get; }
        private const string ServerTimeZoneId = "Eastern Standard Time";
        private readonly ILogger<HangfireService> _logger;        

        public void AddOrUpdate<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default")
        {
            RecurringJob.AddOrUpdate<T>(recurringJobId, methodCall, cronExpression, timeZone, queue);
        }

        public string Enqueue<T>([InstantHandle, NotNull] Expression<Action<T>> methodCall)
        {
            return BackgroundJob.Enqueue<T>(methodCall);
        }
    }
}
