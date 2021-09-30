using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopLabs.Services.Identity.API.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace eShopLabs.Services.Identity.API.Data
{
    public class ConfigurationDbContextSeed
    {
        public async Task SeedAsync(ConfigurationDbContext context, IConfiguration configuration)
        {
            var clientUrls = new Dictionary<string, string>
            {
                ["Mvc"] = configuration.GetValue<string>("MvcClient"),
                ["Spa"] = configuration.GetValue<string>("SpaClient"),
                ["LocationsApi"] = configuration.GetValue<string>("LocationApiClient"),
                ["MarketingApi"] = configuration.GetValue<string>("MarketingApiClient"),
                ["BasketApi"] = configuration.GetValue<string>("BasketApiClient"),
                ["OrderingApi"] = configuration.GetValue<string>("OrderingApiClient"),
                ["WebShoppingAgg"] = configuration.GetValue<string>("WebShoppingAggClient"),
                ["WebhooksApi"] = configuration.GetValue<string>("WebhooksApiClient"),
                ["WebhooksWeb"] = configuration.GetValue<string>("WebhooksWebClient"),
            };

            if (!context.Clients.Any())
            {
                foreach (var client in Config.GetClients(clientUrls))
                {
                    context.Clients.Add(client.ToEntity());
                }

                await context.SaveChangesAsync();
            }
            else
            {
                // Checking always for old redirects to fix existing deployments
                // to use new swagger-ui redirect uri as of v3.0.0
                // There should be no problem for new ones
                // ref: https://github.com/dotnet-architecture/eShopOnContainers/issues/586
                var oldRedirects = (await context.Clients.Include(c => c.RedirectUris).ToListAsync())
                    .SelectMany(c => c.RedirectUris)
                    .Where(r => r.RedirectUri.EndsWith("/o2c.html"))
                    .ToList();

                if (oldRedirects.Any())
                {
                    foreach (var redirect in oldRedirects)
                    {
                        redirect.RedirectUri = redirect.RedirectUri.Replace("/o2c.html", "/oauth2-redirect.html");

                        context.Update(redirect.Client);
                    }

                    await context.SaveChangesAsync();
                }
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var identity in Config.GetIdentityResources())
                {
                    context.IdentityResources.Add(identity.ToEntity());
                }

                await context.SaveChangesAsync();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var api in Config.GetApiResources())
                {
                    context.ApiResources.Add(api.ToEntity());
                }

                await context.SaveChangesAsync();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var scope in Config.GetApiScopes())
                {
                    context.ApiScopes.Add(scope.ToEntity());
                }

                await context.SaveChangesAsync();
            }
        }
    }
}