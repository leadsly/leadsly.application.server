using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Leadsly.Application.Api.Filters
{
    public class AuthAttemptCountAttribute : ResultFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.HttpContext.Request.Headers.ContainsKey("X-Auth-Attempt-Count") == true)
            {
                context.HttpContext.Request.Headers.TryGetValue("X-Auth-Attempt-Count", out StringValues attemptCount);
                if (context.HttpContext.Response.Headers.ContainsKey("X-Auth-Attempt-Count") == false)
                {
                    context.HttpContext.Response.Headers.Add("X-Auth-Attempt-Count", attemptCount);
                }
            }

            base.OnResultExecuting(context);
        }
    }
}
