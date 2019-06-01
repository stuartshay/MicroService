# Build Stage
FROM stuartshay/microservice-api:2.2.2-base AS build-env

ARG BUILD_NUMBER=0
ENV BUILD_NUMBER ${BUILD_NUMBER}

COPY src /app/src
COPY MicroService.sln /app/MicroService.sln

WORKDIR /app/src/MicroService.WebApi

RUN export PATH="$PATH:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-setversion --version 2.0.0 
RUN dotnet tool install --global dotnet-dbinfo --version 1.4.0 
RUN dotnet tool list -g
RUN /root/.dotnet/tools/setversion 2.2.3.${BUILD_NUMBER};

RUN dotnet publish -o /publish -c Release -f netcoreapp2.2 -r debian.9-x64

# Runtime Image Stage
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2

RUN apt-get update && apt-get install -y --no-install-recommends \
        curl \
        && rm -rf /var/lib/apt/lists/* \
        && curl https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh > /wait_for_it.sh \
        && chmod +x /wait_for_it.sh

WORKDIR /publish
COPY --from=build-env /publish .

# Set environment variables
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT Docker

# Open up port
EXPOSE 5000

ENTRYPOINT ["dotnet", "MicroService.WebApi.dll"]
