# Build Stage
FROM mcr.microsoft.com/dotnet/core/sdk:2.2

RUN apt-get update && apt-get install -y --no-install-recommends \
    tree \
 && rm -rf /var/lib/apt/lists/*

COPY .  /app
WORKDIR /app

RUN dotnet restore
