# Microservices Learning On [eShopOnContainers]

## Technical targets

- Net Core 3.1 LTS
- Docker
- Docker-compose
```
# /src
$ docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build -d rabbitmq sqldata
```
- Kubernetes [AKS]
- AzureKeyVault
- AzureServiceBus [prod] [event] [health-check]
- AzureStorage [health-check]
- AzureApplicationInsight
- Serilog [all]
- Autofac
- Polly [retry]
- GRPC
  - Google.Protobuf
  - Grpc.Tools
  - Grpc.AspNetCore.Server
- RabbitMQ [dev] [health-check]
- Migrations
  - [issues01 ]: Unable to create an object of type IntegrationEventLogContext
    - see : create context, eg: [Catalog.API.Infrastructure.IntegrationEventMigrations]
  - [issues02 ]: This package is required for the Entity Framework Core Tools to work.
    - add : [Catalog.API]
   ```xml
    <!-- Add to Catalog.API.csproj -->
    <PropertyGroup>
      <BaseOutputPath BaseOutputPath>..\..\..\BuildingBlocks\EventBus\IntegrationEventLogEF\bin\</BaseOutputPath>
    </PropertyGroup>
   ``` 
  - [Command]
   ```
   # /src/Services/Catalog/Catalog.API
   $ dotnet ef migrations add init -c IntegrationEventLogContext -o Infrastructure/IntegrationEventMigrations
   ```

---

## [Services]

### **Catalog**

---

#### Catalog.API

- CatalogController (api/vi/catalog/)

  - ItemsAsync
  - ItemByIdAsync
  - ItemsWithNameAsync
  - ItemsByTypeIdAndBrandIdAsync
  - ItemsByBrandIdAsync
  - CatalogTypesAsync
  - CatalogBrandsAsync
  - UpdateProductAsync
  - CreateProductAsync
  - DeleteProductAsync

- PicController

  - GetImageAsync

- Grpc

  - dotnet build
  - /obj/Debug/netcoreapp3.1/Proto/CatalogGrpc.cs

- Database

  - CatalogContext
  - IntegrationEventLogContext

- Event libs [shared]
  
  - BuildingBlocks
    - EventBus
    - IntegrationEventLogEF
    - EventBusRabbitMQ   [dev]
    - EventBusServiceBus [prod]

## Dependencies

---

- [Polly - express policies such as Retry, Circuit Breaker, Timeout, Bulkhead Isolation, and Fallback in a fluent and thread-safe manner.](https://github.com/App-vNext/Polly)
- [Serilog](https://github.com/serilog/serilog)
- [Azure.Identity](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme)

# References

- [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers)
- [eShopOnContainers realease 3.1](https://github.com/dotnet-architecture/eShopOnContainers/releases)
- [eShopOnContainers wiki](https://github.com/dotnet-architecture/eShopOnContainers/wiki)
- [Docker_dotnet](https://github.com/dotnet/dotnet-docker/issues/2375)
