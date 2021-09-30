FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src

COPY ["BuildingBlocks/EventBus/EventBus/EventBus.csproj", "BuildingBlocks/EventBus/EventBus/"]
COPY ["BuildingBlocks/EventBus/EventBusRabbitMQ/EventBusRabbitMQ.csproj", "BuildingBlocks/EventBus/EventBusRabbitMQ/"]
COPY ["BuildingBlocks/EventBus/EventBusServiceBus/EventBusServiceBus.csproj", "BuildingBlocks/EventBus/EventBusServiceBus/"]

COPY ["Services/Basket/Basket.API/Basket.API.csproj", "Services/Basket/Basket.API/"]

COPY "docker-compose.dcproj" "docker-compose.dcproj"

COPY "NuGet.config" "NuGet.config"

RUN dotnet restore "Services/Basket/Basket.API/Basket.API.csproj"

COPY BuildingBlocks/EventBus/EventBus/. BuildingBlocks/EventBus/EventBus/.
COPY BuildingBlocks/EventBus/EventBusRabbitMQ/. BuildingBlocks/EventBus/EventBusRabbitMQ/.
COPY BuildingBlocks/EventBus/EventBusServiceBus/. BuildingBlocks/EventBus/EventBusServiceBus/.
COPY Services/Basket/Basket.API/. Services/Basket/Basket.API/.

WORKDIR /src/Services/Basket/Basket.API

RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Basket.API.dll"]
