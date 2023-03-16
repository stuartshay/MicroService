# Microservice API

[![Build/Test/Deploy](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml/badge.svg)](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml) [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-api.svg)](https://hub.docker.com/r/stuartshay/microservice-api/) [![codecov](https://codecov.io/gh/stuartshay/MicroService/branch/master/graph/badge.svg?token=bMKXJXK0Q3)](https://codecov.io/gh/stuartshay/MicroService)

# NYC Open Data GeoSpatial & Data Enrichment API

## Overview

Working with NYC Open Data can be an exciting and rewarding experience, as valuable insights can be gained from the vast resources available in New York City. However, once you begin working with various datasets, you may notice that data is not standardized, codes are used inconsistently across datasets, and can change from version to version. Spatial references can be in either NAD83 or WGS format, and formatting errors may occur in the results.

The goal of this API is to:

1. **Standardize the Data Dictionary**: Establish a consistent data structure and terminology to facilitate seamless integration and analysis of NYC Open Data.
2. **Centralize ESRI Shape Files**: Provide a single repository for ESRI Shape Files, making them easily accessible and organized.
3. **Reduce ETL Workflow**: Streamline the Extract, Transform, Load (ETL) process by storing the data on a traditional file share, making the data accessible to other applications in the stack.

## Key Features

- **Data Standardization**: The API ensures consistent data structure and terminology, making it easier to work with and analyze the NYC Open Data.
- **GeoSpatial Support**: The API provides support for both NAD83 and WGS spatial reference systems, ensuring accurate and consistent geospatial data representation.
- **Centralized Repository**: A single, well-organized location for all ESRI Shape Files related to NYC Open Data.
- **Simplified ETL Workflow**: By storing data on a traditional file share, the API minimizes manual interventions and streamlines the ETL process, while making the data accessible to other applications in the stack.
- **Data Enrichment**: The API offers additional data enrichment capabilities, enhancing the quality and usefulness of the available datasets.

## Technology Stack

## Swagger

![](assets/swagger.png)

## Hosting Environments

### Development

Local Docker Postgres Database

```
cd docker
docker-compose -f docker-compose-local.yml -f docker-compose-metrics.yml pull
docker-compose -f docker-compose-local.yml -f docker-compose-metrics.yml up
```

AWS Postgres Development Database

```
cd docker
docker-compose -f docker-compose-development.yml pull
docker-compose -f docker-compose-development.yml up
```

Swagger API Documentation Page

```
http://<DOCKER_HOST>:5000/swagger/
```

### MyGet/NuGet Packages

| Package              | Status                                                                                                                                                       |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| MicroService.Data    | [![MyGet](https://img.shields.io/myget/microservice/v/MicroService.Data.svg)](https://www.myget.org/feed/microservice/package/nuget/MicroService.Data)       |
| MicroService.Service | [![MyGet](https://img.shields.io/myget/microservice/v/MicroService.Service.svg)](https://www.myget.org/feed/microservice/package/nuget/MicroService.Service) |

### Jenkins Build Status

| Jenkins               | Status                                                                                                                                                                                          |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Docker Base Image     | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-api-base)](https://jenkins.navigatorglass.com/job/MicroService/job/microservice-api-base/)   |
| Docker Deploy Image   | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-api-build)](https://jenkins.navigatorglass.com/job/MicroService/job/microservice-api-build/) |
| Docker x86/Arm7 Image | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-api-multi)](https://jenkins.navigatorglass.com/job/MicroService/job/microservice-api-multi/) |

### Build Commands

| Build Type        | Linux/Mac                    | Windows                       |
| ----------------- | ---------------------------- | ----------------------------- |
| CI Build          | ./build.sh --target=CI-Build | .\build.ps1 --target=CI-Build |
| SonarQube Testing | ./build.sh --target=sonar    | .\build.ps1 --target=sonar    |

**docfx**

```powershell
docfx docfx/docfx.json
docfx docfx/docfx.json -p 9090 --serve

http://localhost:9090
```

## Sonar

```
dotnet sonarscanner begin /k:"MicroService.Api" /d:sonar.host.url="http://192.168.1.172:9100" /d:sonar.exclusions=Program.cs,**/Extensions/**/*.cs  /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml /d:sonar.login="sqp_b0b06eef83dbde81db33f8db66c89554c41f9831"
dotnet build --no-incremental
dotnet-coverage collect 'dotnet test' -f xml  -o 'coverage.xml'
dotnet sonarscanner end /d:sonar.login="sqp_b0b06eef83dbde81db33f8db66c89554c41f9831"
```
