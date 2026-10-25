# Microservice API

[![Build/Test/Deploy](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml/badge.svg)](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml) [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-api.svg)](https://hub.docker.com/r/stuartshay/microservice-api/) [![codecov](https://codecov.io/gh/stuartshay/MicroService/branch/master/graph/badge.svg?token=bMKXJXK0Q3)](https://codecov.io/gh/stuartshay/MicroService) [![Renovate](https://img.shields.io/badge/renovate-enabled-brightgreen.svg)](https://developer.mend.io/github/stuartshay/MicroService)

## NYC Open Data GeoSpatial & Data Enrichment API

[Microservice API Endpoint](https://microservice-api-w6zlqlyoma-uk.a.run.app/)

## Overview

Working with [NYC OpenData](https://opendata.cityofnewyork.us/) offers an exciting opportunity to gain valuable insights from the vast resources available in New York City. However, challenges arise when dealing with various datasets due to a lack of standardization, inconsistent code usage, changing codes between versions, and different spatial reference formats (NAD83 or WGS84). Additionally, formatting errors may occur in the results.

This API aims to:

1. **Standardize the Data Dictionary**: Establish a consistent data structure and terminology for seamless integration and analysis of NYC Open Data.
2. **Centralize ESRI Shape Files**: Provide a single, organized repository for ESRI Shape Files to ensure easy accessibility.
3. **Reduce ETL Workflow**: Streamline the Extract, Transform, Load (ETL) process by storing data on a traditional file share, making it accessible to other applications in the stack.

### Key Features

- **RESTful API**: This API follows the REST architecture, providing a standardized, easy-to-use interface to interact with and manipulate the data.
- **Data Standardization**: The API ensures a consistent data structure and terminology, simplifying the analysis and usage of NYC Open Data.
- **GeoSpatial Support**: With support for both NAD83 and WGS spatial reference systems, the API provides accurate and consistent geospatial data representation. Data is returned in GeoJSON format for easy integration with other applications.
- **Centralized Repository**: A well-organized, centralized location for all ESRI Shape Files related to NYC Open Data.
- **Simplified ETL Workflow**: The API minimizes manual interventions and streamlines the ETL process by storing data on a traditional file share, making it accessible to other applications in the stack.
- **Data Enrichment**: Offering additional data enrichment capabilities, the API enhances the quality and usefulness of the available datasets.

## Technology Stack

- [.NET 10 / ASP.NET Core](https://learn.microsoft.com/aspnet/core/) - Web API (SDK pinned in `global.json`)
- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) - Geometry, shapefile and GeoJSON support
- [ProjNet](https://github.com/NetTopologySuite/ProjNet4GeoAPI) - NAD83 (New York State Plane) and WGS84 conversions
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) and [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) - OpenAPI documentation and API versioning
- [HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks) - `/health` and `/status`
- [Serilog](https://serilog.net/), [OpenTelemetry](https://opentelemetry.io/docs/languages/dotnet/) and [prometheus-net](https://github.com/prometheus-net/prometheus-net) - Logging, tracing and local metrics
- [Cronos](https://github.com/HangfireIO/Cronos) - Scheduled refresh of the in-memory shapefile cache
- [xUnit.net v3](https://xunit.net/) on [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro), with [Moq](https://github.com/devlooped/moq) - Tests and code coverage
- [Docker](https://www.docker.com/) - Container image, published to Docker Hub and Google Artifact Registry
- [Google Cloud Run](https://cloud.google.com/run) - Hosting, with the shapefiles mounted from Cloud Storage
- [GitHub Actions](https://github.com/features/actions) - CI, CodeQL and deployment (workload identity, no service account keys)
- [Renovate](https://docs.renovatebot.com/), [Codecov](https://about.codecov.io/) and [SonarQube](https://www.sonarsource.com/products/sonarqube/) - Dependency updates, coverage and code quality

## Datasets

This project primarily focuses on datasets related to NYC Monuments, Landmarks, and Points of Interest. These datasets are relatively small in size and are versioned together with the source code in this project. By versioning the data and source code together, it simplifies the process of integrating data quality checks for both the data and the source code, ensuring consistency and reliability throughout the project.

- [Shape Files](/files) - NYC Open Data Datasets Shape Files

In production the same files are read from the `files-shapes` Cloud Storage
bucket, which Cloud Run mounts at `/mnt/data/shapes`. Locally and in Docker they
are read from `files/` (see [docker/README.md](docker/README.md)).

## References

- [NYC OpenData](https://opendata.cityofnewyork.us/) - NYC Open Data
- [LPC Designation Reports](https://www.nyc.gov/site/lpc/designations/designation-reports.page) - NYC Landmarks Preservation Commission Designation Reports
- [Stuart Shay's Flickr](https://www.flickr.com/photos/stuartshay) - Stuart Shay's Flickr Collection of NYC Monuments, Landmarks, and Points of Interest

## Local Development

On Debian, Ubuntu, or macOS, install the .NET SDK selected by `global.json` and
the required development tools with:

```bash
./scripts/setup.sh
```

Setup also installs and activates the project's pre-commit hooks. Open a new
terminal after setup, then run:

```bash
make build
make run
```

`make test` extracts the git-ignored building database `.dbf` from its committed
zip the first time it runs. Use `make check` to run every pre-commit check manually. Use `make help` to list
the available targets and configuration options.

## Deployment

A push to `master` that changes `src/`, `test/`, the CI Dockerfile or the deploy
workflow runs [Build/Test/Deploy](.github/workflows/actions.yml): it builds one
amd64 image, pushes it to Docker Hub and Artifact Registry, deploys it to Cloud
Run (`microservice-api`, us-east4) and checks `/health`. Merging to `master` is a
production deploy. Pull requests run [CI](.github/workflows/ci.yml) (lint,
build, analyzers and tests).

The CoreDataStore API and UI depend on the `attributelookup` and
`attributelookupgeojson` endpoints; see the fixed contract in the
[improvement plan](docs/improvement-plan.md).

## Operational endpoints

In Cloud Run, `/health` returns the overall health text and `/status` returns only
`{ "status": "Healthy" }` (HTTP 503 for an unhealthy result). Detailed health
entries and Prometheus `/metrics` are available outside Cloud Run for local
monitoring. Production diagnostics use Cloud Logging and Cloud Monitoring.

See the [improvement plan](docs/improvement-plan.md) for rollout status.
