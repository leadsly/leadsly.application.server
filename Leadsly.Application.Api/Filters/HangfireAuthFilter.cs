using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace Leadsly.Application.Api.Filters
{
    public class HangfireAuthFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context) => true;
    }
}
