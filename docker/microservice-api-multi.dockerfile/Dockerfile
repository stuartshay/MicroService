# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env

ARG BUILD_NUMBER=0
ENV BUILD_NUMBER ${BUILD_NUMBER}

ARG BUILD_ENV=Docker
ENV BUILD_ENV ${BUILD_ENV}

RUN apt-get update && apt-get install -y --no-install-recommends \
    tree \
 && rm -rf /var/lib/apt/lists/*

# Install Additional SDK/Runtimes
RUN apt-get update && apt-get install -y \
    wget \
    apt-transport-https 

RUN wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb 
RUN dpkg -i packages-microsoft-prod.deb 

RUN apt-get update && apt-get install -y \
    dotnet-sdk-3.1 

# Install .NET Tools 
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN dotnet tool install --global dotnet-setversion --version 2.3.0
RUN dotnet tool install --global sempack --version 1.1.7057.22950
RUN dotnet tool install --global dotnet-dbinfo --version 1.4.0 
RUN dotnet tool list -g

COPY .  /app
WORKDIR /app

RUN dotnet restore /p:RestoreUseSkipNonexistentTargets="false"

WORKDIR /app/src/MicroService.WebApi

# Publish ${BUILD_NUMBER}
RUN setversion 5.0.1.125 MicroService.WebApi.csproj 
RUN dotnet publish MicroService.WebApi.csproj -o /publish -c Release -f net5.0  

# Release Stage 
FROM mcr.microsoft.com/dotnet/aspnet:5.0

# Shape Files 
COPY files  /files

WORKDIR /publish
COPY --from=build-env /publish .

# Set environment variables
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT ${BUILD_ENV}

# Open up port
EXPOSE 5000

ENTRYPOINT ["dotnet", "MicroService.WebApi.dll"]
