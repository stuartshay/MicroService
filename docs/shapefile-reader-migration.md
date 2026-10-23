# Shapefile reader migration validation

<!-- cspell:ignore Batey Doña Provi -->

The migration replaces `NetTopologySuite.IO.ShapeFile` 2.1.0 with
`NetTopologySuite.IO.Esri.Shapefile` 1.2.0. Runtime and test projects no longer
reference the legacy package. Service interfaces expose small, application-owned
metadata models rather than reader-library types.

## Differential comparison

On 2026-10-23, an isolated .NET 10 console harness loaded the legacy 2.1.0 package
alongside the candidate service assembly. It read each versioned archive with both
readers and used `test/MicroService.Test/Compatibility/ShapefileSnapshot.cs` to
compare SHA-256 digests of every ordered record's attributes (including .NET types
and nulls), WKB geometry, mapped JSON, and header metadata. Both paths used the
same unchanged mapping methods. The harness also compared the National Register
service's existing Individual Landmark binding separately.

The checked-in `legacy-reader-baseline.json` contains snapshots generated from
the **legacy reader**, not from the replacement. CI recomputes the candidate
snapshots and checks every digest. Updating a versioned archive requires a reviewed
baseline update; do not regenerate expected snapshots from candidate output merely
to pass a failing comparison.

| Dataset | Records | Difference |
| --- | ---: | --- |
| BoroughBoundaries | 5 | None |
| CommunityDistricts | 71 | None |
| DSNYDistricts | 59 | None |
| HistoricDistricts | 156 | None |
| IndividualLandmarkSite | 1,520 | None |
| IndividualLandmarkHistoricDistricts | 37,521 | None |
| NationalRegisterHistoricPlaces | 6,417 | None |
| Neighborhoods | 195 | None |
| NeighborhoodTabulationAreas | 262 | None |
| NypdPolicePrecincts | 77 | None |
| NypdSectors | 302 | None |
| NychaDevelopments | 302 | None |
| Parks | 12,491 | None |
| ScenicLandmarks | 11 | None |
| Subway | 1,928 | None |
| ZipCodes | 263 | None |
| NationalRegisterService | 1,520 | None |

The 16 archives contain **61,580 records**. The separate National Register service
binding comparison covers 1,520 of those records again. No attribute, geometry,
mapping-outcome, or metadata differences remain.

## Compatibility decisions

- Numeric attribute values stay `Double`, even when header metadata reports an
  integer type. Decimal values are parsed with invariant culture to preserve the
  legacy reader's rounding.
- Preserve single `Polygon` output and order multipart shells by ascending shell
  area. Preserve coordinates and holes rather than normalizing or repairing them.
- Use the new reader's `IgnoreInvalidShapes` mode to retain existing invalid
  geometries in the building archive. Strict mode rejects those records; skipping
  or repairing them would change behavior. Baselines verify the complete output.
- Honor explicit `.cpg` encoding. Without one, retain the archives' historical
  Windows-1252 default, including the Parks name “El Batey de Doña Provi Garden”.
- Keep text trimming and all-zero DBF text values compatible. Fixed-width NUL
  strings must remain distinct from null because the existing mappers distinguish
  them.
- Both request-time reads and cron refreshes use the same adapter and dispose
  readers immediately. Cache eviction opens a fresh reader, avoiding reused cursors.

## Existing National Register mismatch

The HRHP archive has a different schema from its named mapper, which expects LPC
Individual Landmark fields. All 6,417 raw HRHP records produce the same missing
`objectid` mapping error with both readers. Snapshots record the error type/message
and count so failures are explicit, not silently dropped. The actual service
currently resolves the Individual Landmark archive; its 1,520 mapped records have
no failures and compare exactly. This pre-existing mismatch is tracked in
[issue #526](https://github.com/stuartshay/MicroService/issues/526). Changing that
binding needs its own response-contract review.

## Pre-merge HTTP checks

Started the candidate API locally in the `CloudRun` environment and compared full
JSON responses against production image `10.0.461`: both LP-00099 lookup contracts
(including GeoJSON coordinates and CORS) and all 16 dataset metadata responses
matched exactly. The candidate returned only overall health at `/status` and 404
at `/metrics`. Local validation passed 428 .NET tests with no skips, four pruning
tests, and the build, analyzer and pre-commit checks.

## Acceptance after merge

Before marking Phase 10 deployed, run both LP-00099 lookup contracts against the
new Cloud Run revision (including GeoJSON geometry and CORS), verify all dataset
metadata responses, and run `npm run test:production:docker` in CoreDataStore.UI.
The migration is not marked deployed until those production checks pass.
