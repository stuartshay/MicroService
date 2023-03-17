# Microservice API

[![Build/Test/Deploy](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml/badge.svg)](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml) [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-api.svg)](https://hub.docker.com/r/stuartshay/microservice-api/) [![codecov](https://codecov.io/gh/stuartshay/MicroService/branch/master/graph/badge.svg?token=bMKXJXK0Q3)](https://codecov.io/gh/stuartshay/MicroService)

# NYC Open Data GeoSpatial & Data Enrichment API

[Microservice API Endpoint](https://microservice-api-w6zlqlyoma-uk.a.run.app/)

## Overview

Working with [NYC OpenData](https://opendata.cityofnewyork.us/) can be an exciting and rewarding experience, as valuable insights can be gained from the vast resources available in New York City. However, once you begin working with various datasets, you may notice that data is not standardized, codes are used inconsistently across datasets, and can change from version to version. Spatial references can be in either NAD83 or WGS84 format, and formatting errors may occur in the results.

The goal of this API is to:

1. **Standardize the Data Dictionary**: Establish a consistent data structure and terminology to facilitate seamless integration and analysis of NYC Open Data.
2. **Centralize ESRI Shape Files**: Provide a single repository for ESRI Shape Files, making them easily accessible and organized.
3. **Reduce ETL Workflow**: Streamline the Extract, Transform, Load (ETL) process by storing the data on a traditional file share, making the data accessible to other applications in the stack.

## Key Features

- **Data Standardization**: The API ensures consistent data structure and terminology, making it easier to work with and analyze the NYC Open Data.
- **GeoSpatial Support**: The API provides support for both NAD83 and WGS spatial reference systems, ensuring accurate and consistent geospatial data representation. Data is returned in GeoJSON format, making it easy to integrate with other applications.

- **Centralized Repository**: A single, well-organized location for all ESRI Shape Files related to NYC Open Data.
- **Simplified ETL Workflow**: By storing data on a traditional file share, the API minimizes manual interventions and streamlines the ETL process, while making the data accessible to other applications in the stack.
- **Data Enrichment**: The API offers additional data enrichment capabilities, enhancing the quality and usefulness of the available datasets.

## Technology Stack

- [Asp.Net core](https://docs.microsoft.com/en-us/aspnet/core/) - C# Web API Framework
- [Net Topology Suite](https://github.com/NetTopologySuite/NetTopologySuite) - Geospatial data structures and algorithms for .NET
- [AutoMapper](https://automapper.org/) - C# Object Mapper
- [HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks) - Health Checks for ASP.NET Core
- [Serilog](https://serilog.net/) - Logging for .NET
- [xUnit.net](https://xunit.net/) - Unit Testing Framework
- [Docker](https://www.docker.com/) - Containerization Platform

## Datasets

The primary focus of the datasets used in this project revolves around NYC Monuments, Landmarks, and Points of Interest. These datasets are relatively small in size and are versioned together with the source code in this project

- [Shape Files](/files) - NYC Open Data Datasets Shape Files

## References

- [NYC OpenData](https://opendata.cityofnewyork.us/) - NYC Open Data
- [LPC Designation Reports](https://www.nyc.gov/site/lpc/designations/designation-reports.page) - NYC Landmarks Preservation Commission Designation Reports
- [Stuart Shay's Flickr](https://www.flickr.com/photos/stuartshay) - Stuart Shay's Flickr Collection of NYC Monuments, Landmarks, and Points of Interest

## Sonar

```
dotnet sonarscanner begin /k:"MicroService.Api" /d:sonar.host.url="http://192.168.1.172:9100" /d:sonar.exclusions=Program.cs,**/Extensions/**/*.cs  /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml /d:sonar.login="sqp_b0b06eef83dbde81db33f8db66c89554c41f9831"
dotnet build --no-incremental
dotnet-coverage collect 'dotnet test' -f xml  -o 'coverage.xml'
dotnet sonarscanner end /d:sonar.login="sqp_b0b06eef83dbde81db33f8db66c89554c41f9831"
```
