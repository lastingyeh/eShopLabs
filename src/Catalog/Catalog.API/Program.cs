using System.Net;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Catalog.API.Extensions;
using Microsoft.Extensions.Options;
using Catalog.API.Infrastructure;

namespace Catalog.API
{
    public class Program
    {
        public static string Namespace = typeof(Startup).Namespace;
        public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
        public static int Main(string[] args)
        {
            // reset configuration & keyvault 
            var configuration = GetConfiguration();
            // configure Serilog 
            Log.Logger = CreateSerilogLogger(configuration);

            try
            {
                Log.Information("Configuration web host ({ApplicationContext})...", Program.AppName);
                // override CreateHostBuilder
                var host = CreateHostBuilder(configuration, args);

                Log.Information("Applying migrations ({ApplicationContext})...", Program.AppName);

                // Migration implement
                host.MigrateDbContext<CatalogContext>((context, services) =>
                {
                    // using Microsoft.Extensions.DependencyInjection;
                    var env = services.GetService<IWebHostEnvironment>();
                    var settings = services.GetService<IOptions<CatalogSettings>>();
                    var logger = services.GetService<ILogger<CatalogContextSeed>>();

                    new CatalogContextSeed().SeedAsync(context, env, settings, logger).Wait();
                });
                // BuildingBlocks\EventBus\IntegrationEventLogEF
                // .MigrateDbContext<IntegrationEventLogContext>((_, __) => { });

                Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);

                host.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);

                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static IWebHost CreateHostBuilder(IConfiguration configuration, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
                .CaptureStartupErrors(false)
                .ConfigureKestrel(options =>
                {
                    var ports = GetDefinedPorts(configuration);

                    options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    });

                    options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http2;
                    });
                })
                .UseStartup<Startup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseWebRoot("Pics")
                .UseSerilog()
                .Build();

        private static (int httpPort, int grpcPort) GetDefinedPorts(IConfiguration configuration)
        {
            var grpcPort = configuration.GetValue("GRPC_PORT", 81);
            var port = configuration.GetValue("PORT", 80);

            return (grpcPort, port);
        }

        private static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            if (config.GetValue<bool>("UseVault", false))
            {
                // var credential = new ClientSecretCredential(
                //     config["Vault:TenantId"],
                //     config["Vault:ClientId"],
                //     config["Vault:ClientSecret"]
                // );

                // builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
                builder.AddAzureKeyVault(
                    $"https://{config["Vault:Name"]}.vault.azure.net/",
                    config["Vault:ClientId"],
                    config["Vault:ClientSecret"]);
            }

            return builder.Build();
        }

        private static Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
        {
            var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            var logstashUrl = configuration["Serilog:LogstashgUrl"];

            return new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", Program.AppName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }
    }
}
