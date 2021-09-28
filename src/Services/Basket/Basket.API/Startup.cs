using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Basket.API.Services;
using eShopLabs.BuildingBlocks.EventBus;
using eShopLabs.BuildingBlocks.EventBus.Abstractions;
using eShopLabs.BuildingBlocks.EventBusRabbitMQ;
using eShopLabs.BuildingBlocks.EventBusServiceBus;
using eShopLabs.Services.Basket.API.Controllers;
using eShopLabs.Services.Basket.API.Infrastructure.Filters;
using eShopLabs.Services.Basket.API.Infrastructure.Middlewares;
using eShopLabs.Services.Basket.API.Infrastructure.Repositories;
using eShopLabs.Services.Basket.API.IntegrationEvents.EventHandling;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var pathBase = Configuration["PATH_BASE"] ?? string.Empty;

            if (!string.IsNullOrEmpty(pathBase))
            {
                app.UsePathBase(pathBase);
            }

            if (env.IsDevelopment())
            {
                app.UseSwaggerUI(opts =>
                {
                    opts.SwaggerEndpoint($"{pathBase}/swagger/v1/swagger.json", "Basket.API V1");
                    opts.OAuthClientId("basketswaggerui");
                    opts.OAuthAppName("Basket Swagger UI");
                });
            }

            app.UseRouting();

            app.UseCors("CorsPolicy");

            app.UseAuth(Configuration);

            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
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
                        UserName = config["EventBusUserName"] ?? default,
                        Password = config["EventBusPassword"] ?? default,
                    };

                    var retryCount = config["EventBusRetryCount"] ?? "5";

                    return new DefaultRabbitMQPersistentConnection(factory, logger, int.Parse(retryCount));
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

                    var retryCount = config["EventBusRetryCount"] ?? "5";

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubscriptionsManager, subscriptionClientName, int.Parse(retryCount));
                });
            }

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            services.AddTransient<ProductPriceChangedIntegrationEventHandler>();
            services.AddTransient<OrderStartedIntegrationEventHandler>();

            return services;
        }

        public static void UseAuth(this IApplicationBuilder app, IConfiguration config)
        {
            if (config.GetValue<bool>("UseLoadTest"))
            {
                app.UseMiddleware<ByPassAuthMiddleware>();
            }

            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
