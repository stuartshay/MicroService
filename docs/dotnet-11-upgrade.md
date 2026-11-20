# .NET 11 Upgrade

A checklist for moving a .NET 10 service to .NET 11, from MicroService's upgrade
(#563, and #564 for OpenAPI). It covers the other .NET repositories as well:
CoreDataStore, CoreDataStore.Reporting, Navigator.Auth, NavigatorAttractionsAPI,
NavigatorFunctions, NavigatorMaps.QL, NavigatorMapsAPI, NavigatorPhotoService,
NavigatorPhotoWorker and NavigatorReportsAPI.

.NET 11 shipped in November 2026 (SDK 11.0.100, runtime 11.0.0). It is a
standard-term release, supported until 2028-11-14, the same date as .NET 10 (LTS),
so there is no support reason to hurry. The reason to move is that the Renovate
updates for .NET 11 (images, `Microsoft.*` packages, some third-party majors) wait
for it.

Microsoft's references:

- [What's new in ASP.NET Core 11](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-11)
- [Breaking changes in ASP.NET Core 11](https://learn.microsoft.com/aspnet/core/breaking-changes/11/overview)
- [Breaking changes in .NET 11](https://learn.microsoft.com/dotnet/core/compatibility/11)
- [OpenAPI in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/overview)

## Status (2026-11-20)

![Live](https://img.shields.io/badge/Live-brightgreen) on .NET 11 in production · ![Done](https://img.shields.io/badge/Done-green) resolved · ![Held](https://img.shields.io/badge/Held-orange) waiting for a stable upstream release ·
![Blocked](https://img.shields.io/badge/Blocked-red) the platform doesn't offer .NET 11 · ![Not needed](https://img.shields.io/badge/Not%20needed-lightgrey) doesn't apply

| Repository | Status | Pull request | Notes |
| --- | --- | --- | --- |
| MicroService | ![Live](https://img.shields.io/badge/Live-brightgreen) | #563, #564 (OpenAPI) | First upgrade; this checklist comes from it |
| CoreDataStore | ![Live](https://img.shields.io/badge/Live-brightgreen) | #334 | EF Core and Npgsql held at 10.x (below) |
| CoreDataStore.Reporting | ![Live](https://img.shields.io/badge/Live-brightgreen) | #60 | EF Core and Npgsql held at 10.x (below) |
| Navigator.Auth | ![Live](https://img.shields.io/badge/Live-brightgreen) | #54 | |
| NavigatorAttractionsAPI | ![Live](https://img.shields.io/badge/Live-brightgreen) | #299 | |
| NavigatorMaps.QL | ![Live](https://img.shields.io/badge/Live-brightgreen) | #109 | With HotChocolate 16 |
| NavigatorMapsAPI | ![Live](https://img.shields.io/badge/Live-brightgreen) | #118 | |
| NavigatorPhotoService | ![Live](https://img.shields.io/badge/Live-brightgreen) | #113 | |
| NavigatorPhotoWorker | ![Live](https://img.shields.io/badge/Live-brightgreen) | #50 | |
| NavigatorReportsAPI | ![Live](https://img.shields.io/badge/Live-brightgreen) | #413 | |
| NavigatorFunctions | ![Blocked](https://img.shields.io/badge/Blocked-red) | | Stays `net10.0`: Cloud Run functions offers `dotnet10` (GA) and `dotnet8` (deprecated), no `dotnet11`. No hurry: .NET 10 is supported until 2028-11-14, as long as .NET 11 |

| Item | Status | Notes |
| --- | --- | --- |
| EF Core and Npgsql 11 | ![Held](https://img.shields.io/badge/Held-orange) | EF Core 11.0.0 is stable, but `Npgsql.EntityFrameworkCore.PostgreSQL` is at 11.0.0-rc.3 and pins an exact EF Core release candidate. CoreDataStore and Reporting stay on EF Core 10.0.15 and Npgsql 10.0.3 until Npgsql 11.0 is stable; then move both together |
| `Asp.Versioning.OpenApi` | ![Held](https://img.shields.io/badge/Held-orange) | 10.2.3 still requires `Microsoft.OpenApi` [2.7.5, 3.0). MicroService registers its API versions itself and suppresses AV0029/AV0030 until a release supports 3.x |
| `Swashbuckle.AspNetCore.Filters` | ![Not needed](https://img.shields.io/badge/Not%20needed-lightgrey) | 11.0.0 is out, but nothing uses it now: MicroService moved to `Microsoft.AspNetCore.OpenApi`, and the five repositories on Swashbuckle 11.0.1 don't use Filters |
| MongoDB driver | ![Done](https://img.shields.io/badge/Done-green) | 3.14.0 runs on .NET 11 in NavigatorAttractionsAPI, NavigatorMapsAPI, NavigatorReportsAPI and Navigator.Auth |
| OpenTelemetry duplicate HTTP tags | ![Not needed](https://img.shields.io/badge/Not%20needed-lightgrey) | No repository calls `AddAspNetCoreInstrumentation()` |

## Change these together, in one pull request

Renovate offers the image, SDK and package updates separately. Merged alone, each
one breaks the build or the deploy, so make them one change:

1. `global.json`: `"version": "11.0.100"` (keep `rollForward: latestFeature`).
2. `<TargetFramework>net11.0</TargetFramework>` in `Directory.Build.props` or every
   project.
3. Every Dockerfile: `mcr.microsoft.com/dotnet/aspnet:11.0` and `sdk:11.0` (with
   the digest, where Renovate pins one). Each Dockerfile copies `global.json`, so an
   SDK 11 image with a 10.0 `global.json`, or the reverse, fails the image build.
   The ASP.NET 11 runtime cannot start a `net10.0` app either.
4. Version numbers that follow the framework: image tags (`10.0.<run>` becomes
   `11.0.<run>`), `/p:Version`, and any `<Version>` in a project file.
5. `Microsoft.*` packages to `11.0.0`. Remove the references .NET 11 now supplies
   through the shared framework: the build warns with NU1510 (MicroService:
   `Microsoft.Extensions.Options`).
6. Every other package to its current version (`dotnet list package --outdated`).
   Read the release notes of each major update (see below).
7. `renovate.json` descriptions or rules that name `net10.0`, the README and the
   Makefile help.

CI picks up the SDK from `global.json` (`actions/setup-dotnet` with
`global-json-file`). Locally, install it next to .NET 10:
`bash dotnet-install.sh --version 11.0.100 --install-dir ~/.dotnet`.

**The image is only built after the merge** in most of these repositories: pull
request CI builds and tests, the deploy workflow builds the image. Build and run
the image locally before merging (see Validation).

## Breaking changes we hit, or need to check

| Change | What to do |
| --- | --- |
| The OpenAPI document includes every `[ProducesResponseType]` per status code, and `[Produces(Type = …)]` counts as one | Two MVC actions that declared different 200 types in `[Produces]` and `[ProducesResponseType]` got `anyOf` schemas. Remove the redundant `Type` from `[Produces]` |
| New analyzer rules (with warnings as errors in `make analyze`) | MicroService: CA1860, `Count == 0` instead of `!Any()`. Run `make analyze` before pushing |
| Microsoft.OpenApi 3.x | Swashbuckle 11 uses it. `Asp.Versioning.OpenApi` (10.x, 10.2.3 on 2026-11-20) still needs 2.x and fails at run time (`MissingMethodException`), not at build time; `Swashbuckle.AspNetCore.Filters` did too until 11.0.0. See OpenAPI below |
| `OpenApiVersion` defaults to 3.2 | Applies to `Microsoft.AspNetCore.OpenApi`. Set `options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1` unless every reader of the document handles 3.2 |
| Hosting emits OpenTelemetry HTTP semantic-convention tags by default | Services that also call `AddAspNetCoreInstrumentation()` may record the same attributes twice. None of ours does (2026-11-20) |
| Kestrel tightens HTTP protocol compliance | Low risk behind Cloud Run; watch for new 400s after the deploy |
| Response compression always sends `Vary: Accept-Encoding` | Only matters where response compression is on |
| ConcurrencyLimiter middleware, MVC compatibility options and obsolete APIs removed | The build fails if a service uses them |

## Package notes

- **CsvHelper 34** is a rewrite: no `ClassMap`, no `CsvHelper.Configuration`. Mark
  each record class `[CsvRecord]` and `partial`, and map columns with attributes
  (`[CsvColumn("Station ID")]`, `[CsvBooleanValues(["1", "2"], ["0"])]`,
  `[CsvNullValues("")]`). The source generator ships in the package. Without
  `[CsvRecord]` the build passes and `GetRecords<T>()` throws at run time, so
  compare the records read before and after the upgrade.
- **Swashbuckle → `Microsoft.AspNetCore.OpenApi`.** Microsoft's own generator has
  been in the templates since .NET 9. MicroService #564 is the worked example:
  - `AddOpenApi("v1")` and `MapOpenApi()` per API version;
  - document, operation and schema transformers instead of Swashbuckle filters;
  - examples ported from Filters;
  - enums listed by name;
  - Swagger UI kept at `/swagger` with `Swashbuckle.AspNetCore.SwaggerUI`;
  - the old `/swagger/v1/swagger.json` route kept as an alias.

  The generator reads the minimal-API JSON options, not the MVC ones, so enums
  written by `JsonStringEnumConverter` in `AddJsonOptions` need a schema
  transformer (or the same converter in `ConfigureHttpJsonOptions`).

  Register each document with a **literal** name (`AddOpenApi("v1", …)`): the
  XML-comment source generator only hooks calls whose document name is a string
  literal, so a loop over version names can silently drop the summaries and
  parameter descriptions (Codex caught this in #564). Test the XML text itself.

  `Asp.Versioning.OpenApi` (one document per API version, which its AV0029/AV0030
  analyzers suggest) still needs `Microsoft.OpenApi` 2.x in its 10.x releases. Until
  it supports 3.x, register the versions yourself and suppress those two IDs with
  the reason.
- **NavigatorFunctions** deploys from source on Cloud Run functions, which offers no
  .NET 11 runtime yet (`gcloud functions runtimes list`: `dotnet10` GA, `dotnet8`
  deprecated, 2026-11-20). It stays on .NET 10 until one appears.
- **EF Core, Npgsql, MongoDB driver:** EF Core 11 waits for a stable Npgsql 11.0 (see
  Status); the MongoDB driver 3.14.0 needed no change.

## Validation

1. `make check`, `make analyze` and `make test` on SDK 11.
2. Run master (.NET 10) and the branch (.NET 11) side by side and compare the
   status and body of every GET endpoint (strip timestamps and `traceId`).
   MicroService compared 25 requests; all matched.
3. Compare the OpenAPI documents operation by operation: paths, parameters,
   response codes and types, examples.
4. For data readers (CSV, shapefiles), compare the records read before and after.
5. Build the deploy image and run it with the production settings (for
   MicroService: `ASPNETCORE_ENVIRONMENT=CloudRun` and the shapes mounted at
   `/mnt/data/shapes`); check `/health` and a few endpoints.
6. After the merge: the deploy is green, the new revision serves 100% of traffic,
   and the service's smoke or contract checks pass.

## Also worth checking in each service

Not .NET 11 changes, but MicroService's upgrade turned these up, and the other
services may share them:

- **Data read from an assembly-relative path.** MicroService's flat files used
  `../../../../../files` from the assembly, which exists in a checkout but not in
  the image; production answered 500 while tests passed (#571). Read data from
  configuration, and run the image with production settings before merging.
- **Probes.** Cloud Run's default is a TCP startup probe only. An HTTP startup
  probe on `/health` every 2 seconds and a liveness probe every 30 seconds
  (#567, #570) add no cold-start time; a 10-second period added 6 seconds.
- **Scale to zero.** State `--min-instances 0` in the deploy flags so a manual
  change doesn't stick (#567).
- **Volumes in the deploy.** A Cloud Storage mount set by hand is lost when the
  service is recreated; repeat `--add-volume` and `--add-volume-mount` in the
  deploy flags (gcloud replaces them by name and path) (#574).
- **Dockerfile.** Run as `USER $APP_UID` (the image's non-root user, as in
  Microsoft's templates), keep the Dockerfile next to the project it builds, and
  lint it with hadolint and Checkov in pre-commit (#566).
