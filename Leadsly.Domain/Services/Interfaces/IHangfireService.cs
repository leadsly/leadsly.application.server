using Hangfire.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Domain.Services.Interfaces
{
    public interface IHangfireService
    {
        public TimeZoneInfo ServerTimeZone { get; }
        public string Enqueue<T>([InstantHandle][NotNull] Expression<Action<T>> methodCall);

        public void AddOrUpdate<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, TimeZoneInfo timeZone = null, string queue = "default");
    }
}
