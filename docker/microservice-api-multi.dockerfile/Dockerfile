# Build Stage
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

ARG BUILD_NUMBER=0
ENV BUILD_NUMBER ${BUILD_NUMBER}

RUN apt-get update && apt-get install -y --no-install-recommends \
    tree \
 && rm -rf /var/lib/apt/lists/*

# Install .NET Tools 
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN dotnet tool install --global dotnet-setversion --version 2.2.0 
RUN dotnet tool install --global dotnet-dbinfo --version 1.4.0 
RUN dotnet tool list -g

COPY .  /app
WORKDIR /app

RUN dotnet restore /p:RestoreUseSkipNonexistentTargets="false"

WORKDIR /app/src/MicroService.WebApi

# Publish
RUN echo ${BUILD_NUMBER}
# RUN /root/.dotnet/tools/setversion 3.1.401.${BUILD_NUMBER}
RUN ls
RUN dotnet publish -o /publish 
## -c Release -f netcoreapp3.1 --version-suffix ${BUILD_NUMBER}

# Release Stage 
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

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
