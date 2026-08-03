FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

ARG APPLICATION_NAME='MicroService.WebApi'
ARG BUILD_DATE='1/1/2023'
ARG BUILD_NUMBER=0

LABEL org.label-schema.application=${APPLICATION_NAME}
LABEL org.label-schema.build-date=${BUILD_DATE}
LABEL org.label-schema.version=10.0.1.${BUILD_NUMBER}

USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY ["global.json", "."]
COPY ["NuGet.config", "."]
COPY ["src/MicroService.WebApi/MicroService.WebApi.csproj", "src/MicroService.WebApi/"]
COPY ["src/MicroService.Common/MicroService.Common.csproj", "src/MicroService.Common/"]
COPY ["src/MicroService.Service/MicroService.Service.csproj", "src/MicroService.Service/"]
COPY ["src/MicroService.Data/MicroService.Data.csproj", "src/MicroService.Data/"]
RUN dotnet restore "src/MicroService.WebApi/MicroService.WebApi.csproj"
COPY . .
WORKDIR "/src/src/MicroService.WebApi"
RUN dotnet build "MicroService.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
ARG BUILD_NUMBER=0
RUN dotnet publish "MicroService.WebApi.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false \
    /p:Version=10.0.1.${BUILD_NUMBER}

FROM base AS final

# Shape Files
COPY files  /files

WORKDIR /app
COPY --from=publish /app/publish .
USER app
ENTRYPOINT ["dotnet", "MicroService.WebApi.dll"]
