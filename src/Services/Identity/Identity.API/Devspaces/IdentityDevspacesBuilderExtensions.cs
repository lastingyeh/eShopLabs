using Microsoft.Extensions.DependencyInjection;

namespace eShopLabs.Services.Identity.API.Devspaces
{
    public static class IdentityDevspacesBuilderExtensions
    {
        public static IIdentityServerBuilder AddDevspacesIfNeeded(this IIdentityServerBuilder builder, bool useDevspaces)
        {
            if (useDevspaces)
            {
                builder.AddRedirectUriValidator<DevspacesRedirectUriValidator>();
            }

            return builder;
        }
    }
}