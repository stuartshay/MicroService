# Cleanup Plan

Status: in progress (2026-10-25). Phases 1, 2 (#533), and 3 (#534) are
complete; Phase 4 (#526) remains.

This plan follows the [improvement plan](improvement-plan.md), which is
complete: every phase of it has merged and is deployed. It comes from a review
of `master` at `3c8a261`, which is what production runs (revision
`microservice-api-00164-ckk`, image `10.0.465`, runtime account
`microservice-api-runtime`). It covers three findings:

1. The shape service registration is spread over five places, and one service
   is already wired to the wrong dataset (#526).
2. The repository still carries PostgreSQL, Jenkins, Kubernetes-era and sample
   DocFX content that no longer matches the service, some of it published on
   GitHub Pages.
3. The improvement plan's phase 9 and 10 status lines described work as "in
   progress" and "awaiting merge" after it had shipped.

Each phase below is one pull request unless noted. Phase 1 is this pull
request. Phases 2 and 3 touch different files and can run in parallel; phase 4
waits for phase 2.

## Fixed contract

The contract in the improvement plan still holds: no phase may change
`POST /api/v1/FeatureService/attributelookup` or
`POST /api/v1/FeatureService/attributelookupgeojson`, and every deploy runs its
contract check (LP-00099 against `HistoricDistricts`). In addition, phase 2 must
not change the responses of `GET /api/v1/FeatureService` (the shape list) or
`GET /api/v1/FeatureService/{id}` (shape metadata) for any shape.

## Phases

### Phase 1: close out the improvement plan (P2)

**Done** in this pull request.

Checked on 2026-10-25 against `master` and production:

- Phase 9: #524 merged on 2026-10-24. In production `/status` returns only
  `{"status":"Healthy"}`, `/metrics` returns 404, the service runs as
  `microservice-api-runtime`, and `microservice-api-develop`, `-master` and
  `-dotnetcore6` no longer exist.
- Phase 10: #527 merged on 2026-10-24. `NetTopologySuite.IO.ShapeFile` is gone
  and `NetTopologySuite.IO.Esri.Shapefile` 1.2.0 is in
  `Directory.Packages.props`. The deploy after #530 routed 100% of traffic to
  `00164-ckk`, and the contract check returns 200.

The improvement plan's status, phase 9 and phase 10 sections now say so. Work
that is still genuinely open moved here (phases 2 to 4) or to the issues listed
at the end.

### Phase 2: one authoritative shape registry (P2)

**Done** in #533 (closes #532; deployed in `microservice-api-00165-7fr`, 100%
traffic confirmed, contract checks and 19/19 `CoreDataStore.UI` production tests
passed).

**Problem.** The mapping from a shape key (`ShapeProperties`) to its dataset
and its service is written out five times:

| Where | What it maps |
| --- | --- |
| `Program.cs`, `ShapefileDataReaderResolver` | key to `ShapeAttribute` (directory, file name), as a 16-branch `if`/`else` chain |
| `Program.cs`, `ShapeServiceResolver` | key to service type, as a 16-arm `switch` |
| Each of the 16 shape services | its own key, hard-coded as `nameof(ShapeProperties.X)` in the constructor |
| `test/MicroService.Test/Fixture/ShapeServiceFixture.cs` | a copy of the reader resolver for tests |
| `InMemoryCacheShapefileCronJobService` | key to file, by reflection over the enum's attributes |

Services are registered by assembly scanning (every type in the
`MicroService.Service.Services` namespace whose name ends in `Service`), while
the flat-file services and both resolvers are registered explicitly.

The copies have already drifted: `NationalRegisterHistoricPlacesService`
resolves `nameof(ShapeProperties.IndividualLandmarkSite)`, so National Register
requests read the Individual Landmark Sites archive (#526). Nothing checks that
the copies agree.

**Change.**

- Add one registry (for example `ShapeRegistry` in `MicroService.Service`):
  a single list of registrations, each with the `ShapeProperties` key and the
  service type, that reads directory, file name and datum from the key's
  existing `ShapeAttribute`. Keep the attributes on the enum: they are also the
  source of the descriptions and styles in `GET /api/v1/FeatureService`.
- Register every shape service explicitly from that list
  (`services.AddScoped(registration.ServiceType)`), and remove the namespace
  scan and the commented-out scan above it.
- Build `ShapefileDataReaderResolver` and `ShapeServiceResolver` from the
  registry. An unknown key still throws `KeyNotFoundException`; the controllers
  validate keys before they resolve, so responses for bad keys don't change.
- Make the cron job and the test fixture use the registry instead of their own
  mappings.
- Keep the National Register binding as it is (it reads `IndividualLandmarkSite`)
  and record it in the registry as a documented exception that points at #526.
  Fixing it changes a response, so it belongs to phase 4.
- Tests: every `ShapeProperties` value is registered exactly once; every
  registered service resolves, and its reader opens the file the registry names;
  the National Register exception is asserted explicitly, so phase 4 has to
  remove it on purpose.

**Done when.** The key-to-dataset and key-to-service mappings exist only in the
registry. `make check`, `make build`, `make test` and `make analyze` pass. The
JSON of `GET /api/v1/FeatureService` and of `GET /api/v1/FeatureService/{id}`
for every shape is identical before and after (save both locally and diff
them). After deploy, the contract check and the CoreDataStore UI production
suite (`npm run test:production:docker` in CoreDataStore.UI) pass.

### Phase 3: remove repository leftovers (P3)

**Done** (#534).

The service has run on Cloud Run with shape files from GCS and no database
since #518. These files describe earlier setups:

| Path | What it is | Decision |
| --- | --- | --- |
| `docker/postgres/` (10 files) | PostgreSQL image, `Jenkinsfile`, init scripts, `pluto.sql`, a 2019 test data CSV | Remove. PostgreSQL left in #518. |
| `docker/docker-compose-cluster.yml` | Compose stack with a PostgreSQL service | Remove the PostgreSQL service, or the file if nothing else in it is used. |
| `docfx/api/Microsoft.Examples.*` (3 files) | Sample controllers from the DocFX template | Remove. |
| `docfx/api/MicroService.*` (166 files) | API reference generated long ago and committed; describes `TestDataRepository`, `ICalculationService` and the AutoMapper profiles, which no longer exist | Remove (see the decision below). |
| `docfx/articles/` | Articles on EKS, Jenkins on AWS, Hyper-V, Kubernetes and MyGet, two PDFs, sample `howto` and `details1` pages | Remove, except anything the owner wants kept for history. |
| `.github/workflows/docfx.yml`, `assets/docfx.png` | Publishes `docfx/` to GitHub Pages | Follows the decision below. |
| `common.props` | Authors, product and an old `3.0.0.1` version, imported by three projects | Move what's still wanted into `Directory.Build.props`; delete the file. |
| `Directory.Packages.props` / project files | `KubernetesClient`, `Flurl`, `AspNetCore.HealthChecks.Uris`, `AspNetCore.HealthChecks.UI`, `AspNetCore.HealthChecks.UI.InMemory.Storage` (in `MicroService.Common`) and `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` (in `MicroService.WebApi`) | Removed each one after `make build` and `make unused` confirmed it unused (`AspNetCore.HealthChecks.UI` also pulled in `KubernetesClient` 15.0.1 transitively). |
| `scripts/update_cloud_run_service.sh` | Adds the `files-shapes` GCS mount to the service by hand | Keep if it's the documented way to recreate the mount; otherwise move the mount into Terraform with the runtime account and remove the script. |

**Owner decision: GitHub Pages.** `https://stuartshay.github.io/MicroService/`
is live and publishes the stale content above (last built 2026-10-23). Either:

- **Retire it (recommended).** Remove `docfx/`, `docfx.yml` and
  `assets/docfx.png`, and turn off Pages. The README, `docs/` and Swagger UI
  already describe the service.
- **Keep it.** Keep only current articles, generate the API reference during
  the workflow instead of committing it, and add the Swagger document.

**Keep.** The local observability stack (`docker/docker-compose-logging.yml`,
`-metrics.yml`, `-tracing.yml` and `docker/grafana`, `prometheus`, `loki`,
`tempo`) stays unless the owner says it's no longer used: Loki and Jaeger still
have configuration switches in the code, off by default. `docker/nginx`,
`docker/gdal`, `auto-approve.yml` (approves Renovate pull requests) and
`scripts/generate-certificates.sh` (used by `make`) stay too.

**Owner step (local only).** This machine has
`credentials/gcp-gh-actions-navigator.json`, a key for `gh-actions-navigator`
(key `d3b8e69e`, disabled since phase 1). It is git-ignored and was never
committed. Delete the file, and delete the disabled key in IAM.

**Done when.** No PostgreSQL, Jenkins or `Microsoft.Examples` content is left;
the GitHub Pages decision is carried out; `make check`, `make build`,
`make test` and `make analyze` pass; the image is no larger than before; and the
contract check passes after deploy.

### Phase 4: National Register service reads its own dataset (P3) — done (#526)

Tracked in #526. After phase 2, change the National Register registration to
`ShapeProperties.NationalRegisterHistoricPlaces` (the `HRHP` archive), give it a
model and mapper for that schema, and remove the documented exception and its
test. First check the request logs for callers of the National Register key,
because its response changes from LPC-shaped fields to National Register fields.

**Done when.** National Register requests read the HRHP archive and return the
documented model with no mapper errors, and the CoreDataStore contracts
(`HistoricDistricts`, `IndividualLandmarkSite`) are unchanged.

## Remaining work as issues

Open one issue for each item, so it is tracked outside this document:

- Phase 2: #532 (closed by #533).
- Phase 3: #534 (closed by #535).
- Phase 4: #526.

## Who can do each phase

MicroService restores from nuget.org only, so cloud agents can build and test
it. Phases 2 and 3 suit cloud agents once the Pages decision is made. Phase 4
needs the request-log check and a decision on the response model first.

## Validation for every pull request

As in the improvement plan: `make check`, `make build`, `make test` and
`make analyze` pass; the Copilot review is answered and every thread resolved;
after a deploying merge the contract check passes and the latest revision
serves 100% of traffic.
