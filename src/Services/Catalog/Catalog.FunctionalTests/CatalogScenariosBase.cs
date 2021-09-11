using System.IO;
using System.Reflection;
using eShopLabs.BuildingBlocks.IntegrationEventLogEF;
using eShopLabs.Services.Catalog.API;
using eShopLabs.Services.Catalog.API.Infrastructure;
using eShops.BuildingBlocks.WebHostCustomization.WebHost.Customization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Catalog.FunctionalTests
{
    public class CatalogScenariosBase
    {
        public TestServer CreateServer()
        {
            var path = Assembly.GetAssembly(typeof(CatalogScenariosBase)).Location;

            var hostBuilder = new WebHostBuilder()
                .UseContentRoot(Path.GetDirectoryName(path))
                .ConfigureAppConfiguration(cb =>
                {
                    cb.AddJsonFile("appsettings.json", optional: false)
                        .AddEnvironmentVariables();
                })
                .UseStartup<Startup>();

            var testServer = new TestServer(hostBuilder);

            testServer.Host.MigrateDbContext<CatalogContext>((ctx, svc) =>
            {
                var env = svc.GetService<IWebHostEnvironment>();
                var settings = svc.GetService<IOptions<CatalogSettings>>();
                var logger = svc.GetService<ILogger<CatalogContextSeed>>();

                new CatalogContextSeed().SeedAsync(ctx, env, settings, logger).Wait();
            })
            .MigrateDbContext<IntegrationEventLogContext>((_, __) => { });

            return testServer;
        }

        public static class Get
        {
            private const int PageIndex = 0;
            private const int PageCount = 4;
            public static string Types = "api/v1/catalog/catalogtypes";
            public static string Brands = "api/v1/catalog/catalogbrands";
            public static string Items(bool paginated = false) =>
                paginated ? "api/v1/catalog/items" + Paginated(PageIndex, PageCount) : "api/v1/catalog/items";

            public static string ItemsById(int id) => $"api/v1/catalog/items/{id}";

            public static string ItemByName(string name, bool paginated = false) =>
                paginated ? "api/v1/catalog/items/withname/{name}" + Paginated(PageIndex, PageCount)
                    : $"api/v1/catalog/items/withname/{name}";
            public static string Filtered(int catalogTypeId, int catalogBrandId, bool paginated = false) =>
                paginated ? $"api/v1/catalog/items/type/{catalogTypeId}/brand/{catalogBrandId}" + Paginated(PageIndex, PageCount)
                    : $"api/v1/catalog/items/type/{catalogTypeId}/brand/{catalogBrandId}";
            private static string Paginated(int pageIndex, int pageCount) =>
                $"?pageIndex={pageIndex}&pageSize={pageCount}";
        }
    }
}