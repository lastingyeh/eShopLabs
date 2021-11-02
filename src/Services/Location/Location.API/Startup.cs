using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLabs.BuildingBlocks.EventBus;
using eShopLabs.BuildingBlocks.EventBus.Abstractions;
using eShopLabs.BuildingBlocks.EventBusRabbitMQ;
using eShopLabs.BuildingBlocks.EventBusServiceBus;
using eShopLabs.Services.Location.API.Controllers;
using eShopLabs.Services.Location.API.Infrastructure.Filters;
using eShopLabs.Services.Location.API.Infrastructure.Repositories;
using eShopLabs.Services.Location.API.Infrastructure.Services;
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
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;

namespace eShopLabs.Services.Location.API
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
            services.AddApplicationInsight(Configuration);

            services.AddCustomHealthCheck(Configuration);

            services.AddControllers(opts =>
            {
                opts.Filters.Add(typeof(HttpGlobalExceptionFilter));
            })
            .AddApplicationPart(typeof(LocationsController).Assembly)
            .AddNewtonsoftJson();

            services.AddAuthService(Configuration);

            services.Configure<LocationSettings>(Configuration);

            services.AddEventBusService(Configuration);

            services.RegisterEventBus(Configuration);

            services.AddSwaggerOpenApiService(Configuration);

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

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddTransient<IIdentityService, IdentityService>();

            services.AddTransient<ILocationsRepository, LocationsRepository>();

            services.AddTransient<ILocationsService, LocationsService>();

            var container = new ContainerBuilder();

            container.Populate(services);

            return new AutofacServiceProvider(container.Build());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

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
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration config)
        {
            var hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            hcBuilder.AddMongoDb(config["ConnectionString"], name: "locations-mongodb-check", tags: new string[] { "mongodb" });

            if (config.GetValue<bool>("AzureServiceBusEnabled"))
            {
                hcBuilder.AddAzureServiceBusTopic(
                    config["EventBusConnection"], topicName: "eshop_event_bus", name: "locations-servicebus-check", tags: new string[] { "servicebus" });
            }
            else
            {
                hcBuilder.AddRabbitMQ($"amqp://{config["EventBusConnection"]}", name: "locations-rabbitmqbus-check", tags: new string[] { "rabbitmqbus" });
            }

            return services;
        }
        public static IServiceCollection AddAuthService(this IServiceCollection services, IConfiguration config)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opts =>
            {
                opts.Authority = config.GetValue<string>("IdentityUrl");
                opts.Audience = "locations";
                opts.RequireHttpsMetadata = false;
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

            return services;
        }
        public static IServiceCollection AddSwaggerOpenApiService(this IServiceCollection services, IConfiguration config)
        {
            services.AddSwaggerGen(opts =>
            {
                opts.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "eShopLabs - Location HTTP API",
                    Version = "v1",
                    Description = "The Location Services HTTP API"
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
                            Scopes = new Dictionary<string, string> { { "Location", "Location API" } },
                        },
                    }
                });

                opts.OperationFilter<AuthorizeCheckOperationFilter>();
            });

            return services;
        }
    }
}
