# Microservice API

[![Build/Test/Deploy](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml/badge.svg)](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml) [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-api.svg)](https://hub.docker.com/r/stuartshay/microservice-api/) [![codecov](https://codecov.io/gh/stuartshay/MicroService/branch/master/graph/badge.svg?token=bMKXJXK0Q3)](https://codecov.io/gh/stuartshay/MicroService)

# NYC Open Data GeoSpatial & Data Enrichment API

[Microservice API Endpoint](https://microservice-api-w6zlqlyoma-uk.a.run.app/)

### Overview

Working with [NYC OpenData](https://opendata.cityofnewyork.us/) offers an exciting opportunity to gain valuable insights from the vast resources available in New York City. However, challenges arise when dealing with various datasets due to a lack of standardization, inconsistent code usage, changing codes between versions, and different spatial reference formats (NAD83 or WGS84). Additionally, formatting errors may occur in the results.

This API aims to:

1. **Standardize the Data Dictionary**: Establish a consistent data structure and terminology for seamless integration and analysis of NYC Open Data.
2. **Centralize ESRI Shape Files**: Provide a single, organized repository for ESRI Shape Files to ensure easy accessibility.
3. **Reduce ETL Workflow**: Streamline the Extract, Transform, Load (ETL) process by storing data on a traditional file share, making it accessible to other applications in the stack.

### Key Features

- **Data Standardization**: The API ensures a consistent data structure and terminology, simplifying the analysis and usage of NYC Open Data.
- **GeoSpatial Support**: With support for both NAD83 and WGS84 spatial reference systems, the API provides accurate and consistent geospatial data representation. Data is returned in GeoJSON format for easy integration with other applications.
- **Centralized Repository**: A well-organized, centralized location for all ESRI Shape Files related to NYC Open Data.
- **Simplified ETL Workflow**: The API minimizes manual interventions and streamlines the ETL process by storing data on a traditional file share, making it accessible to other applications in the stack.
- **Data Enrichment**: Offering additional data enrichment capabilities, the API enhances the quality and usefulness of the available datasets.

## Technology Stack

- [Asp.Net core](https://docs.microsoft.com/en-us/aspnet/core/) - C# Web API Framework
- [Net Topology Suite](https://github.com/NetTopologySuite/NetTopologySuite) - Geospatial data structures and algorithms for .NET
- [AutoMapper](https://automapper.org/) - C# Object Mapper
- [HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks) - Health Checks for ASP.NET Core
- [Serilog](https://serilog.net/) - Logging for .NET
- [xUnit.net](https://xunit.net/) - Unit Testing Framework
- [Docker](https://www.docker.com/) - Containerization Platform

## Datasets

This project primarily focuses on datasets related to NYC Monuments, Landmarks, and Points of Interest. These datasets are relatively small in size and are versioned together with the source code in this project. By versioning the data and source code together, it simplifies the process of integrating data quality checks for both the data and the source code, ensuring consistency and reliability throughout the project.

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
