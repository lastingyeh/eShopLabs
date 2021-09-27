using System;
using eShopLabs.Services.Basket.API.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace eShopLabs.Services.Basket.API.Infrastructure.Exceptions
{
    public static class FailingMiddlewareAppBuilderExtensions
    {
        public static IApplicationBuilder UseFailingMiddleware(this IApplicationBuilder builder)
        {
            return UseFailingMiddleware(builder, null);
        }

        public static IApplicationBuilder UseFailingMiddleware(this IApplicationBuilder builder, Action<FailingOptions> action)
        {
            var opts = new FailingOptions();

            action?.Invoke(opts);

            builder.UseMiddleware<FailingMiddleware>(opts);

            return builder;
        }
    }
}