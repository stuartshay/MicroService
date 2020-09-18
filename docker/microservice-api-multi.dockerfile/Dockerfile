# Build Stage
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env

RUN apt-get update && apt-get install -y --no-install-recommends \
    tree \
 && rm -rf /var/lib/apt/lists/*

COPY .  /app
WORKDIR /app

RUN dotnet restore /p:RestoreUseSkipNonexistentTargets="false"

RUN  cat << \EOF >> ~/.bashrc
RUN  export PATH="$PATH:/root/.dotnet/tools"
RUN EOF
RUN source ~/.bashrc

RUN dotnet tool install --global dotnet-setversion --version 2.2.0 
RUN dotnet tool install --global dotnet-dbinfo --version 1.4.0 
RUN dotnet tool list -g
RUN echo $HOME/.dotnet/tools/setversion



WORKDIR /app/src/MicroService.WebApi

# Set Version & Publish
#RUN $HOME/.dotnet/tools/setversion 3.1.402.${BUILD_NUMBER};
RUN $HOME/.dotnet/tools/setversion --help
#RUN /root/.dotnet/tools/setversion



RUN dotnet publish -o /publish -c Release -f netcoreapp3.1


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
