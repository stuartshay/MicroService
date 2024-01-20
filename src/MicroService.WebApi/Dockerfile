ARG APPLICATION_NAME='MicroService.WebApi'
ARG BUILD_DATE='1/1/2023'
ARG BUILD_NUMBER=0

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

LABEL org.label-schema.application=${APPLICATION_NAME}
LABEL org.label-schema.build-date=${BUILD_DATE}
LABEL org.label-schema.version=8.0.1.${BUILD_NUMBER}

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN rm packages-microsoft-prod.deb

RUN apt-get update && apt-get install -y dotnet-sdk-6.0

ENV PATH="/root/.dotnet/tools:${PATH}"
RUN dotnet tool install --global dotnet-setversion --version 2.5.0
RUN dotnet tool list -g


WORKDIR /src
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
ARG BUILD_NUMBER
RUN setversion 8.0.1.${BUILD_NUMBER}
RUN dotnet publish "MicroService.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final

# Shape Files
COPY files  /files

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MicroService.WebApi.dll"]