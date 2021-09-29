FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src

COPY ["BuildingBlocks/EventBus/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/EventBus/"]
COPY ["BuildingBlocks/EventBus/EventBusRabbitMQ/EventBusRabbitMQ.csproj", "BuildingBlocks/EventBus/EventBusRabbitMQ/"]
COPY ["BuildingBlocks/EventBus/EventBusServiceBus/EventBusServiceBus.csproj", "BuildingBlocks/EventBus/EventBusServiceBus/"]
COPY ["BuildingBlocks/EventBus/IntegrationEventLogEF/IntegrationEventLogEF.csproj", "BuildingBlocks/EventBus/IntegrationEventLogEF/"]
COPY ["BuildingBlocks/WebHostCustomization/WebHost.Customization/WebHost.Customization.csproj", "BuildingBlocks/WebHostCustomization/WebHost.Customization/"]
COPY ["BuildingBlocks/Utils/Utils.csproj", "BuildingBlocks/Utils/"]
COPY ["Services/Catalog/Catalog.API/Catalog.API.csproj", "Services/Catalog/Catalog.API/"]

RUN dotnet restore Services/Catalog/Catalog.API/Catalog.API.csproj

COPY BuildingBlocks/EventBus/EventBus/. BuildingBlocks/EventBus/EventBus/.
COPY BuildingBlocks/EventBus/EventBusRabbitMQ/. BuildingBlocks/EventBus/EventBusRabbitMQ/.
COPY BuildingBlocks/EventBus/EventBusServiceBus/. BuildingBlocks/EventBus/EventBusServiceBus/.
COPY BuildingBlocks/EventBus/IntegrationEventLogEF/. BuildingBlocks/EventBus/IntegrationEventLogEF/.
COPY BuildingBlocks/WebHostCustomization/WebHost.Customization/. BuildingBlocks/WebHostCustomization/WebHost.Customization/.
COPY BuildingBlocks/Utils/. BuildingBlocks/Utils/.
COPY Services/Catalog/Catalog.API/. Services/Catalog/Catalog.API/.

WORKDIR "/src/Services/Catalog/Catalog.API"
RUN dotnet build "Catalog.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Catalog.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalog.API.dll"]
