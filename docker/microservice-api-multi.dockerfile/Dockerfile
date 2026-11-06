ARG BUILD_NUMBER=0
FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base

ENV APPLICATION_NAME=MicroService.WebApi

LABEL org.label-schema.application=${APPLICATION_NAME}
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["global.json", "."]
COPY ["NuGet.config", "."]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/MicroService.WebApi/MicroService.WebApi.csproj", "src/MicroService.WebApi/"]
COPY ["src/MicroService.Common/MicroService.Common.csproj", "src/MicroService.Common/"]
COPY ["src/MicroService.Service/MicroService.Service.csproj", "src/MicroService.Service/"]
COPY ["src/MicroService.Data/MicroService.Data.csproj", "src/MicroService.Data/"]
RUN dotnet restore "./src/MicroService.WebApi/./MicroService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/MicroService.WebApi"

# publish builds the project, so there is no separate build step
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG BUILD_NUMBER
RUN dotnet publish "./MicroService.WebApi.csproj" \
    -c $BUILD_CONFIGURATION -o /app/publish \
    /p:UseAppHost=false /p:Version=10.0.1.${BUILD_NUMBER}

FROM base AS final
USER app

# Shape files are not baked into the image: Cloud Run mounts them from GCS at
# /mnt/data/shapes (appsettings.CloudRun.json), and local runs mount files/ at
# /files (see the docker-compose files and docker/README.md).

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "MicroService.WebApi.dll"]
