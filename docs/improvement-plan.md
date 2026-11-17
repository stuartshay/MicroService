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

- **One OpenAPI document per version from Asp.Versioning.** When
  `Asp.Versioning.OpenApi` supports `Microsoft.OpenApi` 3, switch to
  `WithDocumentPerVersion()` and remove the AV0029/AV0030 suppression in
  `MicroService.WebApi.csproj` (added by #564). Who: agent. Blocker: that release.

## Later

- **Tracing.** Removed on 2026-11-17: it was off everywhere and only wrote to the
  console. If traces are wanted, add OpenTelemetry with an OTLP exporter to Cloud
  Trace (and check ASP.NET Core 11's built-in HTTP tags against the instrumentation).
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
