using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace eShopLabs.Services.Basket.API.Infrastructure.Middlewares
{
    public class ByPassAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private string _currentUserId;
        public ByPassAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;

            switch (context.Request.Path)
            {
                case "/noauth":
                    var userid = context.Request.Query["userid"];

                    if (!string.IsNullOrEmpty(userid))
                    {
                        _currentUserId = userid;
                    }

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/string";

                    await context.Response.WriteAsync($"User set to {_currentUserId}");
                    break;
                case "/noauth/reset":
                    // _currentUserId = null;
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/string";

                    await context.Response.WriteAsync($"User set to none. Token required for protected endpoints.");
                    break;
                default:
                    var authHeader = context.Request.Headers["Authorization"];

                    if (authHeader != StringValues.Empty)
                    {
                        var header = authHeader.FirstOrDefault();
                        var headerValue = "Email ";

                        if (!string.IsNullOrEmpty(header) && header.StartsWith(headerValue) && header.Length > headerValue.Length)
                        {
                            _currentUserId = header.Substring(headerValue.Length);
                        }
                    }

                    if (!string.IsNullOrEmpty(_currentUserId))
                    {
                        var userClaims = new ClaimsIdentity(new[]
                        {
                            new Claim("emails", _currentUserId),
                            new Claim("name", "Test user"),
                            new Claim(ClaimTypes.Name, "Test user"),
                            new Claim("nonce", Guid.NewGuid().ToString()),
                            new Claim("http://schemas.microsoft.com/identity/claims/identityprovider", "ByPassAuthMiddleware"),
                            new Claim(ClaimTypes.Surname, "User"),
                            new Claim("sub", _currentUserId),
                            new Claim(ClaimTypes.GivenName, "Microsoft")
                        }, "ByPassAuth");

                        context.User = new ClaimsPrincipal(userClaims);
                    }

                    await _next.Invoke(context);
                    break;
            }
        }
    }
}