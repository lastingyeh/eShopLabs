using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eShopLabs.Services.Basket.API.Infrastructure.Middlewares
{
    public class FailingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FailingMiddleware> _logger;
        private readonly FailingOptions _opts;
        private bool _mustFail = false;
        public FailingMiddleware(RequestDelegate next, ILogger<FailingMiddleware> logger, FailingOptions opts)
        {
            _opts = opts;
            _logger = logger;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            if (path.Equals(_opts.ConfigPath, StringComparison.OrdinalIgnoreCase))
            {
                await ProcessConfigRequest(context);

                return;
            }

            if (MustFail(context))
            {
                _logger.LogInformation("Response for path {Path} will fail.", path);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "text/plain";

                await context.Response.WriteAsync("Failed due to FailingMiddleware enabled.");

                return;
            }

            await _next.Invoke(context);
        }

        private async Task ProcessConfigRequest(HttpContext context)
        {
            var enable = context.Request.Query.Keys.Any(k => k == "enable");
            var disable = context.Request.Query.Keys.Any(k => k == "disable");

            if (enable && disable)
            {
                throw new ArgumentException("Must use enable or disable querystring values, but not both");
            }

            if (disable)
            {
                _mustFail = false;

                await SendOkResponse(context, "FailingMiddleware disabled. Further requests will be processed.");

                return;
            }

            if (enable)
            {
                _mustFail = true;

                await SendOkResponse(context, "FailingMiddleware enabled. Further requests will return HTTP 500");

                return;
            }

            await SendOkResponse(context, string.Format("FailingMiddleware is {0}", _mustFail ? "enabled" : "disabled"));

            return;
        }

        private async Task SendOkResponse(HttpContext context, string message)
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/plain";

            await context.Response.WriteAsync(message);
        }

        private bool MustFail(HttpContext context)
        {
            var path = context.Request.Path.Value;

            if (_opts.NotFilteredPaths.Any(p => p.Equals(path, StringComparison.InvariantCultureIgnoreCase)))
            {
                return false;
            }

            return _mustFail && (_opts.EndpointPaths.Any(x => x == path) || _opts.EndpointPaths.Count == 0);
        }
    }
}