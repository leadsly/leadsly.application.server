﻿using Leadsly.Api.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Leadsly.Application.Api.Configurations
{
    public static class ApiDefaults
    {
        public static void Configure(MvcOptions options)
        {
            options.Filters.Clear();

            options.Filters.Add<GlobalControllerExceptionAttribute>();
            options.Filters.Add<InvalidModelStateFilter>(FilterOrders.RequestValidationFilter);
            options.Filters.Add(new ProducesAttribute("application/json"));

            AuthorizationPolicy defaultPolicy = new AuthorizationOptions().DefaultPolicy;
            options.Conventions.Add(new BearerTokenAuthorizeConvention(defaultPolicy));
        }
    }
}
