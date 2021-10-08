using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLabs.BuildingBlocks.EventBus;
using eShopLabs.BuildingBlocks.EventBus.Abstractions;
using eShopLabs.BuildingBlocks.EventBusRabbitMQ;
using eShopLabs.BuildingBlocks.EventBusServiceBus;
using eShopLabs.Services.Basket.API.Controllers;
using eShopLabs.Services.Basket.API.Grpc;
using eShopLabs.Services.Basket.API.Infrastructure.Filters;
using eShopLabs.Services.Basket.API.Infrastructure.Middlewares;
using eShopLabs.Services.Basket.API.Infrastructure.Repositories;
using eShopLabs.Services.Basket.API.IntegrationEvents.EventHandling;
using eShopLabs.Services.Basket.API.IntegrationEvents.Events;
using eShopLabs.Services.Basket.API.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace eShopLabs.Services.Basket.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // ApplicationInsight & Grpc
            services.AddApplicationInsight(Configuration)
                .AddGrpc(opts =>
                {
                    opts.EnableDetailedErrors = true;
                });

            // Controller & filters
            services.AddControllers(opts =>
            {
                opts.Filters.Add(typeof(HttpGlobalExceptionFilter));
                opts.Filters.Add(typeof(ValidateModelStateFilter));
            })
            .AddApplicationPart(typeof(BasketController).Assembly)
            .AddNewtonsoftJson();

            // Swagger Open Api 
            services.AddSwaggerOpenApiService(Configuration);

            // OAuth JWT
            services.AddAuthService(Configuration);

            // Health check [redis, azure service bus, rabbitmq]
            services.AddCustomHealthCheck(Configuration);

            // Settings
            services.AddOptions();

            services.Configure<BasketSettings>(Configuration);

            // Redis
            services.AddRedisService(Configuration);

            // EventBus connections & Register EventBus Handler
            services.AddEventBusService(Configuration)
                .RegisterEventBus(Configuration);

            // Cors policy
            services.AddCors(opts =>
            {
                opts.AddPolicy("CorsPolicy", builder =>
                {
                    builder.SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });

            // HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Repository
            services.AddTransient<IBasketRepository, RedisBasketRepository>();

            // User Identity
            services.AddTransient<IIdentityService, IdentityService>();

            var container = new ContainerBuilder();

            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            var pathBase = Configuration["PATH_BASE"];

            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            if (env.IsDevelopment())
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint. 
                app.UseSwagger().UseSwaggerUI(opts =>
                {
                    logger.LogInformation($"swagger path: {pathBase}/swagger/v1/swagger.json");

                    opts.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "Basket.API V1");
                    opts.OAuthClientId("basketswaggerui");
                    opts.OAuthAppName("Basket Swagger UI");

                    // opts.InjectJavascript("../assets/swaggerinit.js");
                });
            }

            app.UseRouting();

            app.UseCors("CorsPolicy");

            // [env] prod / dev / test 
            ConfigureAuth(app);

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<BasketService>();
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();

                endpoints.MapGet("/_proto/", async ctx =>
                {
                    ctx.Response.ContentType = "text/plain";

                    using var fs = new FileStream(Path.Combine(env.ContentRootPath, "Proto", "basket.proto"), FileMode.Open, FileAccess.Read);
                    using var sr = new StreamReader(fs);

                    while (!sr.EndOfStream)
                    {
                        var line = await sr.ReadLineAsync();

                        if (line != "/* >>" || line != "<< */")
                        {
                            await ctx.Response.WriteAsync(line);
                        }
                    }
                });

                endpoints.MapHealthChecks("/hc", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                });

                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self"),
                });
            });

            app.SubscribeEventBus();
        }

        protected virtual void ConfigureAuth(IApplicationBuilder app)
        {
            if (Configuration.GetValue<bool>("UseLoadTest"))
            {
                app.UseMiddleware<ByPassAuthMiddleware>();
            }

            app.UseAuthentication();
            app.UseAuthorization();
        }
    }

    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddApplicationInsight(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApplicationInsightsTelemetry(configuration);

            services.AddApplicationInsightsKubernetesEnricher();

            return services;
        }

        public static IServiceCollection AddSwaggerOpenApiService(this IServiceCollection services, IConfiguration config)
        {
            services.AddSwaggerGen(opts =>
            {
                opts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "eShopLabs - Basket HTTP API",
                    Version = "v1",
                    Description = "The Basket Services HTTP API"
                });

                opts.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{config.GetValue<string>("IdentityUrlExternal")}/connect/authorize"),
                            TokenUrl = new Uri($"{config.GetValue<string>("IdentityUrlExternal")}/connect/token"),
                            Scopes = new Dictionary<string, string> { { "basket", "Basket API" } },
                        },
                    }
                });

                opts.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            return services;
        }

        public static IServiceCollection AddAuthService(this IServiceCollection services, IConfiguration config)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");

            var identityUrl = config.GetValue<string>("IdentityUrl");

            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.Authority = identityUrl;
                opts.RequireHttpsMetadata = false;
                opts.Audience = "basket";
            });

            return services;
        }

        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration config)
        {
            var builder = services.AddHealthChecks();

            builder.AddCheck("self", () => HealthCheckResult.Healthy());
            builder.AddRedis(config["ConnectionString"], name: "redis-check", tags: new string[] { "redis" });

            if (config.GetValue<bool>("AzureServiceBusEnabled"))
            {
                builder.AddAzureServiceBusTopic(
                    config["EventBusConnection"], topicName: "eshop_event_bus",
                    name: "basket-servicebus-check", tags: new string[] { "servicebus" });
            }
            else
            {
                builder.AddRabbitMQ(
                    $"amqp://{config["EventBusConnection"]}", name: "basket-rabbitmqbus-check", tags: new string[] { "rabbitmqbus" });
            }

            return services;
        }

        public static IServiceCollection AddRedisService(this IServiceCollection services, IConfiguration config)
        {
            services.AddSingleton<ConnectionMultiplexer>(sp =>
                {
                    var settings = sp.GetRequiredService<IOptions<BasketSettings>>().Value;
                    var config = ConfigurationOptions.Parse(settings.ConnectionString, true);

                    config.ResolveDns = true;

                    return ConnectionMultiplexer.Connect(config);
                });

            return services;
        }

        public static IServiceCollection AddEventBusService(this IServiceCollection services, IConfiguration config)
        {
            if (config.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IServiceBusPersisterConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultServiceBusPersisterConnection>>();
                    var serviceBusConnection = new ServiceBusConnectionStringBuilder(config["EventBusConnection"]);

                    return new DefaultServiceBusPersisterConnection(serviceBusConnection, logger);
                });
            }
            else
            {
                services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                    var factory = new ConnectionFactory
                    {
                        HostName = config["EventBusConnection"],
                        DispatchConsumersAsync = true,
                    };

                    if (!string.IsNullOrEmpty(config["EventBusUserName"]))
                    {
                        factory.UserName = config["EventBusUserName"];
                    }

                    if (!string.IsNullOrEmpty(config["EventBusPassword"]))
                    {
                        factory.Password = config["EventBusPassword"];
                    }

                    var retryCount = 5;

                    if (!string.IsNullOrEmpty(config["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(config["EventBusRetryCount"]);
                    }

                    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                });
            }

            return services;
        }

        public static IServiceCollection RegisterEventBus(this IServiceCollection services, IConfiguration config)
        {
            var subscriptionClientName = config["SubscriptionClientName"];

            if (config.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
                {
                    var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                    var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    return new EventBusServiceBus(serviceBusPersisterConnection, logger, eventBusSubscriptionsManager, subscriptionClientName, iLifetimeScope);
                });
            }
            else
            {
                services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
                {
                    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    var retryCount = 5;

                    if (!string.IsNullOrEmpty(config["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(config["EventBusRetryCount"]);
                    }

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
                });
            }

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            services.AddTransient<ProductPriceChangedIntegrationEventHandler>();
            services.AddTransient<OrderStartedIntegrationEventHandler>();

            return services;
        }

        public static void SubscribeEventBus(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>();
            eventBus.Subscribe<OrderStartedIntegrationEvent, OrderStartedIntegrationEventHandler>();
        }
    }
}
