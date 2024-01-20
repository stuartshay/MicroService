ARG APPLICATION_NAME='MicroService.WebApi'
ARG BUILD_DATE='1/1/2023'
ARG BUILD_NUMBER=0

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

LABEL org.label-schema.application=${APPLICATION_NAME}
LABEL org.label-schema.build-date=${BUILD_DATE}
LABEL org.label-schema.version=8.0.1.${BUILD_NUMBER}

USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

ENV PATH="/root/.dotnet/tools:${PATH}"
RUN dotnet tool install --global dotnet-setversion --version 3.0.0
RUN dotnet tool list -g

WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["src/MicroService.WebApi/MicroService.WebApi.csproj", "src/MicroService.WebApi/"]
COPY ["src/MicroService.Common/MicroService.Common.csproj", "src/MicroService.Common/"]
COPY ["src/MicroService.Service/MicroService.Service.csproj", "src/MicroService.Service/"]
COPY ["src/MicroService.Data/MicroService.Data.csproj", "src/MicroService.Data/"]
RUN dotnet restore "./src/MicroService.WebApi/./MicroService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/MicroService.WebApi"
RUN dotnet build "./MicroService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG BUILD_NUMBER
RUN setversion 8.0.1.${BUILD_NUMBER}
RUN dotnet publish "./MicroService.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final

# Shape Files
COPY files  /files
RUN ls /files


WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroService.WebApi.dll"]