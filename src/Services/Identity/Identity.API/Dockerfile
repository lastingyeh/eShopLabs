FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

WORKDIR /src

COPY ["BuildingBlocks/WebHostCustomization/WebHost.Customization/WebHost.Customization.csproj", "BuildingBlocks/WebHostCustomization/WebHost.Customization/"]
COPY ["BuildingBlocks/Utils/Utils.csproj", "BuildingBlocks/Utils/"]

COPY ["Services/Identity/Identity.API/Identity.API.csproj", "Services/Identity/Identity.API/"]

COPY "docker-compose.dcproj" "docker-compose.dcproj"

COPY "NuGet.config" "NuGet.config"

RUN dotnet restore "Services/Identity/Identity.API/Identity.API.csproj"

COPY BuildingBlocks/WebHostCustomization/WebHost.Customization/. BuildingBlocks/WebHostCustomization/WebHost.Customization/.
COPY BuildingBlocks/Utils/. BuildingBlocks/Utils/.
COPY Services/Identity/Identity.API/. Services/Identity/Identity.API/.

WORKDIR /src/Services/Identity/Identity.API

RUN dotnet publish --no-restore -c Release -o /app

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Identity.API.dll"]