using System;
using eShopLabs.Services.Basket.API.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace eShopLabs.Services.Basket.API.Infrastructure.Middlewares
{
    public class FailingStartupFilter : IStartupFilter
    {
        private readonly Action<FailingOptions> _optsAction;
        public FailingStartupFilter(Action<FailingOptions> optsAction)
        {
            _optsAction = optsAction;
        }
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) =>
            app =>
            {
                app.UseFailingMiddleware(_optsAction);
                
                next(app);
            };
    }
}