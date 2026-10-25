# Improvement Plan

Status: complete (updated 2026-10-25). Every phase has merged and is deployed:
production revision `microservice-api-00164-ckk` (image `10.0.465`) runs
`master` at `3c8a261`. Follow-up work is in the [cleanup plan](cleanup-plan.md).
This plan comes from a review of `master` at
`fd5e6f9` and of the production service. It brings MicroService in line with
the changes made to the CoreDataStore API in October 2026, in a sequence of
small pull requests.

Each phase below is one pull request unless noted. Work through them in order:
phase 1 comes first because no other change can be deployed until it lands.
Mark a phase done here when its pull request merges.

## Baseline at the initial review

### What the service does in practice

Requests to `microservice-api` over the 30 days before the review (Cloud Run
request log):

| Requests | Endpoint | Caller |
| --- | --- | --- |
| 3,130 | `POST /api/v1/FeatureService/attributelookup` | CoreDataStore API (`ShapeServiceClient`, user agent `HttpClient`) |
| 2,561 | `POST /api/v1/FeatureService/attributelookupgeojson` | Browsers: the CoreDataStore UI map |
| 1,758 | `OPTIONS /api/v1/FeatureService/attributelookupgeojson` | CORS preflight for the request above |
| fewer than 20 | everything else (Swagger, `GET /api/v1/FeatureService`, `/health`) | people and probes |

- No 5xx responses. Latency p50 0.01 s, p95 0.22 s.
- 68 responses were 4xx: lookups with no match (404) and invalid
  `attributelookupgeojson` bodies (400).
- Only 6 requests were scanner probes (CoreDataStore sees about 3,000 a week).

### Health of the code

- `make build` and `make analyze`: 0 warnings, 0 errors.
- `make test`: 308 passed, 9 skipped, once the test data step below has run.
  Without it, 191 of 317 tests fail (see phase 2).
- No vulnerable packages (`dotnet list package --vulnerable --include-transitive`).
- 21 packages are outdated (see phase 6).

### Production configuration

- Cloud Run service `microservice-api`, project `velvety-byway-327718`,
  region `us-east4`. 1 CPU, 2 GiB, concurrency 80, startup CPU boost,
  maximum 100 instances (the default).
- Runs as the default Compute Engine service account.
- Shape files come from the GCS bucket `files-shapes`, mounted at
  `/mnt/data/shapes` (`ShapeConfiguration:ShapeRootDirectory` in
  `appsettings.CloudRun.json`).
- `ConnectionStrings__PostgreSql` is set from a secret. Only
  `TestDataController` and `PercentileController` use it.
- Images: Artifact Registry `microservice-api` (cleanup policy: keep the 5
  newest images, delete older ones after 7 days) and Docker Hub
  `stuartshay/microservice-api` (131 tags, about 28 GB listed, back to 2020).
  The current image is 290 MB compressed.

## Fixed contract

CoreDataStore depends on the attribute lookup endpoints. No phase may change
them, and every deploy is checked against them.

`POST /api/v1/FeatureService/attributelookup`, request:

```json
{
  "key": "HistoricDistricts",
  "attributes": [{ "Key": "LPNumber", "Value": "LP-00099" }]
}
```

Response: a JSON array of features. CoreDataStore reads `lpNumber`, `areaName`,
`boroName`, `boroCode`, `shapeArea`, `shapeLength` and `boundingBox.centre.x`
/ `.y`; it treats 404 as "no feature". The shape keys CoreDataStore sends are
`IndividualLandmarkSite` and `HistoricDistricts`.

`POST /api/v1/FeatureService/attributelookupgeojson` takes the same request and
returns GeoJSON for the UI map. It is called from browsers, so CORS must keep
allowing the CoreDataStore UI origins (phase 2).

Check after each deploy (expect 200 and a feature for LP-00099):

```bash
curl -s -X POST https://microservice-api-w6zlqlyoma-uk.a.run.app/api/v1/FeatureService/attributelookup \
  -H 'Content-Type: application/json' \
  -d '{"key":"HistoricDistricts","attributes":[{"Key":"LPNumber","Value":"LP-00099"}]}'
```

## Phases

### Phase 1: deploy with workload identity (P1)

**Done** in #511 (and stuartshay/nyc-landmarks-vector-db#450 for the identity pool and deployer account).

**Problem.** `.github/workflows/actions.yml` authenticates to GCP with a JSON
service account key (`secrets.GH_ACTIONS_SERVICE_ACCOUNT`, account
`gh-actions-navigator`). Every user-managed key on that account is disabled,
so the next deploy will fail at `google-github-actions/auth`. The last
successful deploy was 2026-08-08. Each deploy also passes
`--allow-unauthenticated`, which rewrites the service's IAM policy and needs
`run.services.setIamPolicy`.

**Change.**

- Authenticate with workload identity federation, as CoreDataStore does since
  stuartshay/CoreDataStore#269: `google-github-actions/auth` with
  `workload_identity_provider: ${{ vars.GCP_WORKLOAD_IDENTITY_PROVIDER }}` and
  `service_account: ${{ vars.GCP_DEPLOYER }}`, and `permissions: id-token: write`
  on the jobs that authenticate.
- Remove `--allow-unauthenticated` from the deploy flags. The public invoker
  binding stays on the service; manage it in Terraform with the rest.
- Remove the `GH_ACTIONS_SERVICE_ACCOUNT` secret once a deploy succeeds.

**Owner steps.** The workload identity pool and deploy accounts are managed
in Terraform in stuartshay/nyc-landmarks-vector-db
(`infrastructure/github_deploy/main.tf`). Add this repository to the provider's
allowed repositories, give its deploy account Artifact Registry writer on
`microservice-api`, `run.developer` and `iam.serviceAccountUser` on the
runtime account, and apply. Then set the repository variables
`GCP_WORKLOAD_IDENTITY_PROVIDER` and `GCP_DEPLOYER`.

**Done when.** A push to `master` deploys a new revision, and the contract
check passes.

### Phase 2: quick wins (P2)

**Done** in #514 and #517. Production now caps at 10 instances, allows only the CoreDataStore origins, builds and pushes directly without intermediate tar artifacts or gcsfuse, and serves a smaller image (was 291 MB).

One pull request; each item is small.

- **Instance cap.** Add `--max-instances 10` to the deploy flags. Normal load
  uses one instance.
- **CORS.** Replace `AllowAnyOrigin` in `AddCorsConfiguration` with a
  configured list (`Cors:AllowedOrigins`): `https://coredatastore.com`, the
  CoreDataStore UI Cloud Run URL
  (`https://coredatastore-ui-w6zlqlyoma-uk.a.run.app`, used by its post-deploy
  smoke test) and `http://localhost:5173`, `:4173` and `:2000` for local UI
  development. With no origins configured, allow none. Remove the unused named
  policy. The old `navigator-ui` and `navigatormaps-ui` bundles do not
  reference this service. Same approach as stuartshay/CoreDataStore#282.
- **Logging.** `appsettings.CloudRun.json` sets `Logging:LogLevel:Default` to
  `Debug`. Use `Information`.
- **HTTPS redirect.** Cloud Run terminates TLS before the container. Skip
  `UseHttpsRedirection` when the environment is `CloudRun`, as CoreDataStore
  does.
- **Smaller image.** The Dockerfile copies `files/` (551 MB) into the image,
  but production reads shape files from the GCS mount. Remove `COPY files /files`
  for the Cloud Run image; keep a way to run locally with the files (a Docker
  Compose volume or a separate local target).
- **Local test data.** `make test` fails until the `.dbf` that CI extracts in
  its "Prepare test data" step exists. Run the same `unzip` from `make test`
  (skip it when the file is already there).

**Done when.** The image is smaller than 290 MB compressed; `make test` passes
on a fresh clone; the CoreDataStore UI map still loads boundaries (its
production Playwright suite passes); a request from another origin gets no
`Access-Control-Allow-Origin` header; the service shows a maximum of 10
instances.

### Phase 3: Docker Hub tag pruning (P2)

**Done** in #521 and #523 (2026-10-23). Configured the repository's
`DOCKERHUB_PRUNE_TOKEN` secret and removed 135 historical tags in the initial
cleanup. The pagination fix tolerates Docker Hub advertising a cached next page
that no longer exists. Deployment #461 at commit `ff1e21d` verified automatic
pruning: one old release deleted, leaving `latest` and releases `10.0.457` through
`10.0.461`.
Local Compose examples use `latest` so retention does not remove their image tag.

- Copy `scripts/dockerhub-prune.sh` from stuartshay/CoreDataStore (#283 and its
  rate-limit fix). It keeps `latest` and the 5 newest tags.
- Run it in the Docker Hub publish job after the push, with
  `continue-on-error: true` and the `DOCKERHUB_PRUNE_TOKEN` secret.
- Run it once by hand to remove the backlog (about 125 tags).

**Owner step completed.** Configured the `DOCKERHUB_PRUNE_TOKEN` repository
secret using the existing pruning credential.

**Done when.** Docker Hub keeps `latest` and the 5 newest tags after a deploy.

### Phase 4: remove the test data endpoints and PostgreSQL (P2)

**Done** in #518. Removed `TestDataController`, `PercentileController`, `CalculationService`, `TestDataRepository`, the `ConnectionStrings:PostgreSql` startup requirement, `Npgsql`, `Dapper` and `AspNetCore.HealthChecks.NpgSql`, along with the remaining skipped test (`FunctionHelperIntegrationTest`).

`TestDataController` (`api/v1/TestData`) and `PercentileController` are the
only users of `ITestDataRepository`, `CalculationService` and the PostgreSQL
connection. They received no traffic in the review period.

- Remove the two controllers, `CalculationService`, `TestDataRepository`,
  `ITestDataRepository` and their tests, and the PostgreSQL health check if
  one is registered.
- Stop requiring `ConnectionStrings:PostgreSql` at startup.
- Remove Npgsql and Dapper if nothing else uses them.

**Owner step completed** (2026-10-23). Removed the
`ConnectionStrings__PostgreSql` secret binding from the production Cloud Run
service. The removal created revision `microservice-api-00157-k8p` using the
existing `10.0.459` image. Health and attribute-lookup contract checks passed
without the binding. The underlying Secret Manager secret was not deleted.

**Done when.** The service starts without a database connection string and
the contract check passes.

### Phase 5: skipped tests (P2)

**Done** in #512. It also fixed the datum of the Individual Landmark & Historic District building data, which is WGS84, not NAD83.

Nine tests are skipped with `TODO`/`FIX` reasons. Fix each or delete it with a
reason in the pull request:

| Test | File |
| --- | --- |
| Calculate_Percentile (integration) | `Functions/FunctionHelperIntegrationTest.cs` (goes with phase 4) |
| Get Feature Attribute Lookup | `Integration/CommunityDistrictServiceTest.cs` |
| Get Geospatial Point Lookup | `Integration/IndividualLandmarkHistoricDistrictsTest.cs` |
| GetFeatureCollection Mapped Historic District | `Integration/IndividualLandmarkHistoricDistrictsTest.cs` |
| Get Feature Attribute Lookup ("Not Filtering") | `Integration/NypdPolicePrecinctServiceTest.cs` |
| Get Geospatial Point Lookup Not Found | `Integration/SubwayServiceTest.cs` |
| FindPointsByRadius_ReturnsPointsWithinRadius | `Integration/SubwayServiceTest.cs` |
| Validate Mappings | `Mappings/BoroughBoundaryShapeTest.cs` |
| Validate Mappings | `Mappings/CommunityDistrictShapeTest.cs` |

Start with the NYPD precinct lookup ("Not Filtering" suggests a real bug in
attribute lookup) and the Historic District mapping, which is on the
CoreDataStore path. Add a test for the fixed contract above that runs on every
build.

### Phase 6: packages (P3)

**Done** in #519. Moved all NuGet versions to `Directory.Packages.props`, updated all 21 outdated packages, migrated `xunit.v3` 4.0 onto `Microsoft.Testing.Platform`, and aligned `renovate.json` package groups.

- Move package versions to `Directory.Packages.props` (central package
  management), as in CoreDataStore.
- Update the 21 outdated packages, including OpenTelemetry 1.17 to 1.20,
  Swashbuckle 10.2 to 10.3, and xunit.v3 3.2 to 4.0 (a major version: check
  its breaking changes). Keep Renovate's groups in step with the new layout.
- Leave `NetTopologySuite.IO.ShapeFile` as it is; phase 10 replaces it.

### Phase 7: replace AutoMapper (P3)

**Done** in #515. All mapped values are unchanged except Parks `System` and `LandUse`, which are now `""` instead of `null` when the source value is missing.

AutoMapper 16 is commercially licensed. Replace the profiles under
`MicroService.Service/Mappings` with explicit mapping methods, as CoreDataStore
does (`EntityMappings`, `LpcReportMapper`), keeping the mapping tests.

### Phase 8: workflow hardening (P3)

**Done** in #513.

- Pin all 36 action references by commit SHA with the version as a comment
  (Renovate's `helpers:pinGitHubActionDigests` keeps them current), as in
  stuartshay/CoreDataStore#257.
- Give each workflow the least `permissions` it needs.

### Phase 9: production cleanups (P3)

**Done** in #524 (2026-10-24) and stuartshay/nyc-landmarks-vector-db#451.
Verified in production on 2026-10-25: `/status` returns only
`{"status":"Healthy"}`, `/metrics` returns 404, the service runs as
`microservice-api-runtime`, and the three old services are gone.

Operational evidence (2026-10-23):

- Applied the reviewed, targeted Terraform plan for the runtime account, the
  `files-shapes` object-reader grant, log/metric writer grants, and deployer
  impersonation. A subsequent targeted plan reports no changes.
- Tested candidate revision `microservice-api-00160-sox` on image `10.0.461` under
  `microservice-api-runtime@velvety-byway-327718.iam.gserviceaccount.com` before
  routing 100% of production traffic to it. Health, both LP-00099 lookup contracts,
  CORS, and borough reads passed.
- Retired `microservice-api-develop`, `microservice-api-master`, and
  `microservice-api-dotnetcore6`. Source references, domain mappings, regional
  scheduled HTTP callers, uptime checks, and URL maps had no dependencies; the
  last 30 days contained only seven curl probes, including failing health checks.
- Added integration tests that verify Cloud Run exposes only overall health,
  preserves HTTP 503 on failure, and returns 404 for `/metrics`, while local
  diagnostic endpoints remain available.

Implementation decisions:

- Define the dedicated runtime identity and its IAM grants in the existing
  Terraform root in `nyc-landmarks-vector-db/infrastructure/github_deploy`.
  Grant object read access only on `files-shapes`, plus log and metric writing;
  remove the deployer's permission to run as the default Compute Engine account.
- Check source references, recent request logs, domain mappings, and scheduled
  callers before deleting the three obsolete services.
- In Cloud Run, keep `/health` public, reduce `/status` to `{ "status": "Healthy" }`
  (or the unhealthy result and HTTP 503), and do not expose `/metrics`. Keep
  detailed health and Prometheus endpoints in local development. Cloud Run
  request logging and Google Cloud monitoring remain available.
- Preserve the underlying PostgreSQL secret; removing a service binding does not
  establish that no other service uses the secret.

- **Old Cloud Run services.** `microservice-api-develop`,
  `microservice-api-master` and `microservice-api-dotnetcore6` received 2 to 3
  requests each in 30 days, and their images were deleted. Delete the services.
- **Runtime service account.** Replace the default Compute Engine account with
  a dedicated account that can only read the `files-shapes` bucket (and write
  logs and metrics).
- **Public operational endpoints.** `/status` (health details) and `/metrics`
  are public. Decide whether that is acceptable, as was decided for
  CoreDataStore.

### Phase 10: replace the legacy shapefile reader (P3)

**Done** in #527 (2026-10-24); production runs it since #530 routed traffic to
`microservice-api-00164-ckk`. The National Register mismatch it found is #526,
planned as phase 4 of the [cleanup plan](cleanup-plan.md). Replaced the package, introduced application-owned metadata models, and routed
request-time and cron refresh reads through one compatibility adapter. The
independent legacy-versus-candidate comparison matched all 61,580 records across
16 archives, plus the existing National Register service binding. Saved regression
snapshots cover attributes and types, WKB geometry, mapped outcomes, and metadata.
See [migration validation](shapefile-reader-migration.md) for provenance, results,
compatibility decisions, and the existing National Register mismatch (#526).
The contract check passed after deploy.

`NetTopologySuite.IO.ShapeFile` 2.1.0 (April 2022) is deprecated on NuGet as
"Legacy". Its replacement is
[`NetTopologySuite.IO.Esri.Shapefile`](https://github.com/NetTopologySuite/NetTopologySuite.IO.Esri)
(1.2.0, September 2024; targets .NET Standard 2.0 and needs only
`NetTopologySuite` and `NetTopologySuite.Features` 2.x, which the service
already uses). Do it after phase 6, so the package moves straight into
`Directory.Packages.props`.

Where the old reader is used:

- `MicroService.Service/Helpers/CachedShapefileDataReader.cs` inherits
  `ShapefileDataReader` and exposes its `ShapefileHeader` and `DbaseFileHeader`
  through `IShapefileDataReaderService`.
- `MicroService.Service/Extensions/ShapefileDataReaderExtensions.cs`
  (`ReadFeatures`) turns records into `Feature` objects with an
  `AttributesTable` keyed by the `.dbf` field names, and sets `BoundingBox` to
  the geometry envelope.
- `MicroService.WebApi/Services/InMemoryCacheShapefileCronJobService.cs`
  creates a `ShapefileDataReader` to refresh the cache.
- `IShapeService.GetShapeProperties()` and `GetShapeDatabaseProperties()`
  return the library's header types to `FeatureServiceController`, whose
  `GET /api/v1/FeatureService/{id}` builds `NumFields`, `NumRecords`, `Bounds`,
  `LastUpdatedDate` and `FieldsList` (field name and .NET type name) from them.
- Tests: the fixture, the integration tests, `Unit/ShapeFileTest.cs` and
  `Unit/WktReaderFunctions.cs`.

Changes:

- Read features with the Esri reader (`Shapefile.ReadAllFeatures` or
  `ShapefileReader`), keeping one `Feature` per record with the same attribute
  names and the same `BoundingBox`.
- Replace `ShapefileHeader` and `DbaseFileHeader` in the service interfaces
  with the service's own small model (record count, fields, bounds,
  last-updated date), so the API no longer exposes library types. Keep the
  `GET /api/v1/FeatureService/{id}` response the same.
- Remove `NetTopologySuite.IO.ShapeFile`.

Risks to check, because the two readers can return the same file differently:

- **Attribute value types.** Numeric, date and text `.dbf` fields may come back
  as different .NET types (for example `int` or `decimal` instead of `double`),
  with different trimming of padded text or different nulls. The mappers parse
  most values from `ToString()`, but `FieldsList` reports the type names.
- **Geometry types.** The new reader may return every polygon as a
  `MultiPolygon`. GeoJSON from `attributelookupgeojson` must keep the same
  geometry types, or the CoreDataStore UI map must be checked with the new ones.
- **Text encoding.** Names with non-ASCII characters must read the same
  (three datasets have a `.cpg` file).

Compare before and after the way #515 was checked: map every feature of every
dataset with both readers and diff the results, and compare the
`attributelookupgeojson` output for LP-00099.

**Done when.** The old package is gone, the full comparison shows no
unexplained differences, and the contract check and the CoreDataStore UI
production suite pass after deploy.

### Later

- **Rate limiting.** Low priority while scanner traffic stays near zero. If it
  grows, reuse stuartshay/CoreDataStore#285 (200 requests a minute per client
  IP, taken from the last `X-Forwarded-For` entry).
- **Request cancellation.** Cloud Run does not pass client disconnects to the
  container (verified for CoreDataStore), so passing `CancellationToken`
  through would not save work in production.

## Validation for every pull request

From `.github/instructions/pull-requests.instructions.md`:

- `make check`, `make build`, `make test`, `make analyze` pass.
- Copilot review completed and every thread answered and resolved.
- After a merge that deploys: the contract check above passes, and for phases
  2 to 4 the CoreDataStore UI production suite passes
  (`npm run test:production:docker` in CoreDataStore.UI).
