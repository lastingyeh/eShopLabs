# Catalog services

## APIs
---
### CatalogController [api/v1/catalog]
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

### PicController [api/v1/catalog/items/{catalogItemId:int}/pic]

- GetImagesAsync

## Database
---

- CatalogContext
- IntegrationEventLogContext

## Sharing libs
---

- EventBus
- IntegrationEventLogEF
- EventBusRabbitMQ   [dev]
- EventBusServiceBus [prod]


## Contents
---

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
- Serilog
- Autofac
- Polly [health-check] [retry]
- GRPC
  - Google.Protobuf
  - Grpc.Tools
  - Grpc.AspNetCore.Server
- RabbitMQ [dev] [health-check]
- Migrations
  - [issues01]: Unable to create an object of type IntegrationEventLogContext
    - create context, eg: [Catalog.API.Infrastructure.IntegrationEventMigrations]
  - [issues02]: This package is required for the Entity Framework Core Tools to work.
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
## Test
---
  - Unit test
  ```
  # create unit test project 
  # /Catalog
  $ dotnet new xunit -n Catalog.UnitTests -f netcoreapp3.1

  # test
  # /Catalog/Catalog.UnitTest
  $ dotnet test
  ```
