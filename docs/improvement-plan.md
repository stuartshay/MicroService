# Improvement Plan

All phases are complete; see the [archive](archive/improvement-plan-2026-11.md)
for the baseline review and phases 1 to 10 (#511 to #527). Open follow-up work
is in the [cleanup plan](cleanup-plan.md).

## Owner steps

| Item | Owner | Notes |
| --- | --- | --- |
| None left from this plan | — | The cleanup plan lists the owner steps still open |

## Next

Nothing. Every phase merged and shipped.

## Later

- **Rate limiting.** Low priority while scanner traffic stays near zero. If it
  grows, reuse stuartshay/CoreDataStore#285 (200 requests a minute per client
  IP, taken from the last `X-Forwarded-For` entry).
- **Request cancellation.** Cloud Run does not pass client disconnects to the
  container, so passing `CancellationToken` through would not save work.

## Validation for every pull request

- `make check`, `make build`, `make test` and `make analyze` pass.
- The Copilot review is complete and every thread is answered and resolved.
- After a deploying merge, the contract check passes (expect 200 and a feature
  for LP-00099), and the latest revision serves 100% of traffic:

```bash
curl -s -X POST https://microservice-api-w6zlqlyoma-uk.a.run.app/api/v1/FeatureService/attributelookup \
  -H 'Content-Type: application/json' \
  -d '{"key":"HistoricDistricts","attributes":[{"Key":"LPNumber","Value":"LP-00099"}]}'
```
