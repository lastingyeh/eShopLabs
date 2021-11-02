using Microsoft.AspNetCore.Http;

namespace eShopLabs.Services.Location.API.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _context;
        public IdentityService(IHttpContextAccessor context)
        {
            _context = context;
        }
        public string GetUserIdentity() => _context.HttpContext.User.FindFirst("sub").Value;
    }
}