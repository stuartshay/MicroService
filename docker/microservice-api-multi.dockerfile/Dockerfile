# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

ARG BUILD_NUMBER=0
ENV BUILD_NUMBER ${BUILD_NUMBER}

ARG BUILD_ENV=Docker
ENV BUILD_ENV ${BUILD_ENV}

RUN apt-get update && apt-get install -y --no-install-recommends \
    tree \
    && rm -rf /var/lib/apt/lists/*

# Install .NET Tools
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN dotnet tool install --global dotnet-setversion --version 2.4.0
RUN dotnet tool list -g

COPY .  /app
WORKDIR /app

RUN dotnet restore /p:RestoreUseSkipNonexistentTargets="false"

WORKDIR /app/src/MicroService.WebApi

# Publish ${BUILD_NUMBER}
RUN echo $BUILD_NUMBER
#RUN setversion 6.0.1.${BUILD_NUMBER}
RUN dotnet publish MicroService.WebApi.csproj -o /publish -c Release -f net7.0

# Release Stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0

# Shape Files
COPY files  /files

WORKDIR /publish
COPY --from=build-env /publish .

# Set environment variables
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT Docker

# Open up port
EXPOSE 5000

ENTRYPOINT ["dotnet", "MicroService.WebApi.dll"]
