# Microservice API

[![Build/Test/Deploy](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml/badge.svg)](https://github.com/stuartshay/MicroService/actions/workflows/actions.yml) [![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-api.svg)](https://hub.docker.com/r/stuartshay/microservice-api/) [![codecov](https://codecov.io/gh/stuartshay/MicroService/branch/master/graph/badge.svg?token=bMKXJXK0Q3)](https://codecov.io/gh/stuartshay/MicroService) [![Renovate](https://img.shields.io/badge/renovate-enabled-brightgreen.svg)](https://developer.mend.io/github/stuartshay/MicroService)

## NYC Open Data GeoSpatial & Data Enrichment API

A production geospatial API for New York City open data. It answers "which
landmark, historic district, precinct or neighborhood is this?" by attribute or
by coordinate, from 16 standardized NYC shapefile datasets, and returns JSON or
GeoJSON in either NAD83 (New York State Plane) or WGS84.
[CoreDataStore](https://coredatastore.com) uses it in production for landmark
boundaries and map outlines.

[API endpoint and Swagger UI](https://microservice-api-w6zlqlyoma-uk.a.run.app/)

## Overview

NYC Open Data is a rich source, but its datasets are hard to combine: field
names and codes differ between datasets and change between versions, and some
are published in NAD83 while others use WGS84. This API:

1. **Standardizes the data dictionary**: every dataset is mapped to a typed,
   documented model with consistent names (for example `lpNumber`, `boroName`,
   `boroCode`).
2. **Centralizes the ESRI shapefiles**: one versioned set of files, served
   through one API.
3. **Removes ETL steps**: consumers query the shapefiles through the API instead
   of loading them into their own databases.

### What it serves

| Endpoint | Purpose |
| --- | --- |
| `GET /api/v1/FeatureService` | The available datasets, with descriptions and map styles |
| `GET /api/v1/FeatureService/{id}` | Dataset metadata: record count, bounds, fields and types |
| `GET /api/v1/FeatureService/featureList` | Every feature in a dataset |
| `GET /api/v1/FeatureService/geospatiallookup` | The feature that contains a point (NAD83 or WGS84) |
| `POST /api/v1/FeatureService/attributelookup` | Features matching attribute values, as JSON |
| `POST /api/v1/FeatureService/attributelookupgeojson` | The same lookup as GeoJSON, for maps |
| `GET /api/v1/FlatFileService/{id}` | Subway station and station complex lookups |
| `GET /api/v1/TransformationService/ConvertNad83ToWgs84` and `ConvertWgs84ToNad83` | Coordinate conversion |

Datasets include Historic Districts, Individual Landmark Sites, the LPC
Individual Landmark and Historic District Building Database, Scenic Landmarks,
the National Register of Historic Places, Borough Boundaries, Community
Districts, Neighborhoods, Neighborhood Tabulation Areas, NYPD Precincts and
Sectors, DSNY Districts, NYCHA Developments, Parks, Subway Entrances and ZIP
Codes.

### Who depends on it

The [CoreDataStore](https://github.com/stuartshay/CoreDataStore) API calls
`attributelookup` to enrich landmark reports with district and site details,
and the CoreDataStore UI calls `attributelookupgeojson` from the browser to
draw landmark and historic district outlines on its map. Those two endpoints are
a fixed contract: every deploy checks them, and changes to them are coordinated
with CoreDataStore.

## Architecture

```text
GitHub Actions ──(workload identity)──▶ Artifact Registry ──▶ Cloud Run: microservice-api
                                                              │  runs as microservice-api-runtime
CoreDataStore API ──▶ attributelookup ────────────────────────┤
CoreDataStore UI  ──▶ attributelookupgeojson (CORS) ──────────┤
                                                              ▼
                                          Cloud Storage bucket files-shapes
                                          mounted read-only at /mnt/data/shapes
                                          └─ features cached in memory, refreshed on a schedule
```

### Why this design

- **Files, not a database.** The datasets are small, read-only and change only
  when NYC publishes a new version. Serving the published shapefiles directly
  keeps them identical to the source, with nothing to load, migrate or keep in
  sync. The service has no database dependency.
- **Mounted from Cloud Storage.** The image holds only the application, so it
  stays small and a data update doesn't need a new build. The same files are in
  `files/` for local development and tests.
- **In-memory cache.** Each dataset is read once and kept in memory, so lookups
  don't touch storage. A scheduled job (Cronos) and file watchers refresh the
  cache when the files change.
- **One registry for datasets.** `ShapeRegistry` maps each dataset key to its
  files and its service, and tests check that every dataset is registered once
  and opens the file it names.

### Production

- **Hosting:** Cloud Run (`microservice-api`, us-east4), capped at 10 instances.
  Cloud Run terminates TLS.
- **Deployment:** GitHub Actions signs in with workload identity federation; no
  service account keys. The deploy account can only deploy this service and
  push its images.
- **Runtime identity:** a dedicated account, `microservice-api-runtime`, that
  can only read the `files-shapes` bucket and write logs and metrics.
- **CORS:** only the CoreDataStore origins (and local UI development ports) may
  call the API from a browser.
- **Operational endpoints:** `/health` returns the overall health, and `/status`
  returns only `{ "status": "Healthy" }` (HTTP 503 when unhealthy). Detailed
  health entries and Prometheus `/metrics` are available only outside Cloud Run;
  production diagnostics use Cloud Logging and Cloud Monitoring.

## Quality and security

- **Tests:** 436 xUnit v3 tests on Microsoft.Testing.Platform, including
  contract tests for the CoreDataStore endpoints and a saved snapshot of every
  dataset that each test run compares against. Coverage is about 97% (Codecov).
- **Static analysis:** .NET analyzers in the build (`make analyze`), CodeQL on
  every push, pull request and weekly schedule, CodeFactor, and SonarQube
  (`make sonar`).
- **Pre-commit:** gitleaks secret scanning, actionlint, shellcheck, codespell,
  cspell and markdownlint.
- **Dependencies:** Renovate, with central package management
  (`Directory.Packages.props`). Major .NET and observability image updates wait
  for approval on the Dependency Dashboard.
- **CI hardening:** every GitHub Action is pinned to a commit SHA, and workflows
  run with read-only permissions unless a job needs more.
- **Deploy checks:** after each deploy the workflow routes all traffic to the new
  revision, confirms it, and checks `/health`.

## Technology stack

- [.NET 10 / ASP.NET Core](https://learn.microsoft.com/aspnet/core/) - Web API (SDK pinned in `global.json`)
- [NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) and [NetTopologySuite.IO.Esri](https://github.com/NetTopologySuite/NetTopologySuite.IO.Esri) - Geometry, shapefile and GeoJSON support
- [ProjNet](https://github.com/NetTopologySuite/ProjNet4GeoAPI) - NAD83 (New York State Plane) and WGS84 conversions
- [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) and [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) - OpenAPI documentation and API versioning
- [HealthChecks](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks) - `/health` and `/status`
- [Serilog](https://serilog.net/), [OpenTelemetry](https://opentelemetry.io/docs/languages/dotnet/) and [prometheus-net](https://github.com/prometheus-net/prometheus-net) - Logging, tracing and local metrics
- [Cronos](https://github.com/HangfireIO/Cronos) - Scheduled refresh of the in-memory shapefile cache
- [xUnit.net v3](https://xunit.net/) on [Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro), with [Moq](https://github.com/devlooped/moq) - Tests and code coverage
- [Docker](https://www.docker.com/) - Container image, published to Docker Hub and Google Artifact Registry
- [Google Cloud Run](https://cloud.google.com/run) - Hosting, with the shapefiles mounted from Cloud Storage
- [GitHub Actions](https://github.com/features/actions) - CI, CodeQL and deployment
- [Renovate](https://docs.renovatebot.com/), [Codecov](https://about.codecov.io/) and [SonarQube](https://www.sonarsource.com/products/sonarqube/) - Dependency updates, coverage and code quality

## Datasets

The datasets cover NYC monuments, landmarks, points of interest and the
boundaries around them. They are small, so they are versioned with the source
code: a data update goes through the same review, tests and regression
baselines as a code change.

- [Shape Files](/files) - NYC Open Data shapefiles

In production the same files are read from the `files-shapes` Cloud Storage
bucket, which Cloud Run mounts at `/mnt/data/shapes`. Locally and in Docker they
are read from `files/` (see [docker/README.md](docker/README.md)).

## Local development

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
zip the first time it runs. Use `make check` to run every pre-commit check
manually. Use `make help` to list the available targets and configuration
options.

## Deployment

A push to `master` that changes `src/`, `test/`, the build files
(`global.json`, `Directory.Build.props`, `Directory.Packages.props`,
`NuGet.config`), the image Dockerfile or the deploy workflow runs
[Build/Test/Deploy](.github/workflows/actions.yml). It builds one amd64 image,
pushes it to Docker Hub and Artifact Registry, deploys it to Cloud Run, routes
all traffic to the new revision and checks `/health`. Merging to `master` is a
production deploy. Pull requests run [CI](.github/workflows/ci.yml) (lint,
build, analyzers and tests) and CodeQL.

## Project history

The service was modernized in 2026: workload identity deploys, a dedicated
runtime account, .NET 10 with central package management, AutoMapper and the
legacy shapefile reader replaced, PostgreSQL and other unused infrastructure
removed, and the shape registry consolidated. The
[improvement plan](docs/improvement-plan.md) and
[cleanup plan](docs/cleanup-plan.md) record each step, and
[shapefile-reader-migration.md](docs/shapefile-reader-migration.md) documents how
the reader replacement was validated.

## References

- [NYC OpenData](https://opendata.cityofnewyork.us/) - NYC Open Data
- [LPC Designation Reports](https://www.nyc.gov/site/lpc/designations/designation-reports.page) - NYC Landmarks Preservation Commission Designation Reports
- [Stuart Shay's Flickr](https://www.flickr.com/photos/stuartshay) - Stuart Shay's Flickr Collection of NYC Monuments, Landmarks, and Points of Interest
