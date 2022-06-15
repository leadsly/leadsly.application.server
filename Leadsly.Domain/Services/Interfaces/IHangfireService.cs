using Hangfire.Annotations;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IHangfireService
    {
        public TimeZoneInfo ServerTimeZone { get; }
        public string Enqueue<T>([InstantHandle][NotNull] Expression<Action<T>> methodCall);
        public string Schedule<T>([InstantHandle][NotNull] Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);
        public string Schedule<T>([NotNull][InstantHandle] Expression<Action<T>> methodCall, DateTimeOffset enqueueAt);
        public bool Delete([NotNull] string jobId);
        public void AddOrUpdate<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");        
    }
}
