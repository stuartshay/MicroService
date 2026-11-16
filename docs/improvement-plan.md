# Improvement Plan

Open work only (updated 2026-11-16). The finished work is in [the archive](archive/):
the October 2026 [improvement plan](archive/improvement-plan-2026-11.md) (phases 1 to
10), the [cleanup plan](archive/cleanup-plan-2026-11.md) (phases 1 to 4; its last open
items moved here), the [shapefile reader migration](archive/shapefile-reader-migration.md)
record, and phase 0 of the [shapefile refresh](archive/shapefile-refresh-plan-2026-11.md).
The shapefile refresh itself (phases 1 to 4) has its own
[plan](shapefile-refresh-plan.md). Merging to `master` deploys production.

**Agent**: repository-only work in one pull request that stops for review.
**Claude**: production and deploy work, and reviewing agent pull requests.
**Owner**: decisions, repository settings and credentials.

## Owner steps

| Step | Detail |
| --- | --- |
| Delete the disabled key `d3b8e69e` | A key of `gh-actions-navigator`, disabled and unused since the deploys moved to workload identity. GitHub Pages, the `develop` and `gh-pages` branches and the local `credentials/` file are already gone (checked 2026-11-16) |

## Next

- **.NET 11** (#563). SDK 11.0.100, `net11.0`, the 11.0 images and the package
  updates, including CsvHelper 34. Who: owner merges, Claude checks the deploy.
  The checklist for the other .NET services is in
  [.NET 11 upgrade](dotnet-11-upgrade.md).
- **OpenAPI from `Microsoft.AspNetCore.OpenApi`** (#564, stacked on #563).
  Replaces Swashbuckle's generator; Swagger UI stays at `/swagger`. Who: owner
  merges after #563, Claude checks the deploy.
- **Flat-file endpoints answer 500 in production.** `GET /api/v1/FlatFileService/{id}`
  (the subway stations and station complexes CSVs) fails with
  `DirectoryNotFoundException` for `/files/Subway_Station_Locations/Stations.csv`:
  production reads shapes from `/mnt/data/shapes`, and the CSVs are in neither the
  image nor the bucket. Read them from the shapes root (they are already under
  `files/`, so the publish job copies them) and add a smoke check. Who: Claude or
  agent. Blocker: none.
- **OpenTelemetry after .NET 11.** ASP.NET Core 11 adds the HTTP semantic-convention
  tags to the server activity by itself, and the service also calls
  `AddAspNetCoreInstrumentation()`. Once #563 is live, check the traces for
  duplicate attributes, and drop the instrumentation or set the
  `Microsoft.AspNetCore.Hosting.SuppressActivityOpenTelemetryData` switch. Who: Claude.
- **The `files-shapes` mount in the deploy.** The deploy workflow doesn't set the
  bucket mount; the service keeps it from `scripts/update_cloud_run_service.sh`,
  run by hand. Add the volume and mount to the deploy flags, then remove the
  script. Who: Claude, with the owner's approval (production). Blocker: none.
- **One OpenAPI document per version from Asp.Versioning.** When
  `Asp.Versioning.OpenApi` supports `Microsoft.OpenApi` 3, switch to
  `WithDocumentPerVersion()` and remove the AV0029/AV0030 suppression in
  `MicroService.WebApi.csproj` (added by #564). Who: agent. Blocker: that release.

- **Logging and tracing settings left from the local stack.** The Docker Compose
  stack (Loki, Promtail, Grafana, Prometheus, Tempo, Jaeger) was removed on
  2026-11-16. `GrafanaLokiConfiguration` and `JaegerConfiguration` in
  `appsettings*.json` are off but still point at it, and `MicroService.Common`
  still references `Serilog.Sinks.Grafana.Loki`. Remove them, or point them at a
  real backend (Cloud Run already sends the console logs to Cloud Logging). Who: agent or Claude. Blocker: none.

- **An HTTP health probe on Cloud Run.** The service has only Cloud Run's default
  TCP startup probe on port 5000; `/health` is checked once, by the deploy. Add an
  HTTP startup probe and a liveness probe on `/health` to the deploy flags. This is
  also why Checkov's CKV_DOCKER_2 (a Dockerfile `HEALTHCHECK`, which Cloud Run
  ignores) is skipped in `.checkov.yaml`. Who: Claude, with the owner's approval
  (production). Blocker: none.

## Later

- **Rate limiting.** Low priority while scanner traffic stays near zero. If it
  grows, reuse stuartshay/CoreDataStore#285 (200 requests a minute per client
  IP, taken from the last `X-Forwarded-For` entry).
- **Request cancellation.** Cloud Run does not pass client disconnects to the
  container (verified for CoreDataStore), so passing `CancellationToken`
  through would not save work in production.

## Validation for every pull request

- `make check`, `make build`, `make test` and `make analyze` pass.
- The Codex review is complete and every thread is answered and resolved.
- After a deploying merge, the contract check passes (expect 200 and a feature
  for LP-00099), and the latest revision serves 100% of traffic:

```bash
curl -s -X POST https://microservice-api-w6zlqlyoma-uk.a.run.app/api/v1/FeatureService/attributelookup \
  -H 'Content-Type: application/json' \
  -d '{"key":"HistoricDistricts","attributes":[{"Key":"LPNumber","Value":"LP-00099"}]}'
```
