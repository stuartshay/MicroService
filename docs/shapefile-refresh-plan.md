# Shapefile Refresh Plan

<!-- cspell:ignore Ditmas Douglaston HRHP MODZCTA Queensboro Willoughby ZCTA ZCTAs borocd borocode calda damg desda dsny enfh gfnw gkve gpmc gthc hcne landmarkty lpnumb mpzq ncre nybb nycd nynta nypp phvi planimetric pvji qexa qhxs sitede sitest skyk xbvj yuvp -->

Phase 0 (a merge to `master` publishes `files/` to the production bucket) is
done; see the [archive](archive/shapefile-refresh-plan-2026-11.md) (PRs 551,
552, 554, 555 and 556). Phases 1 to 4 bring the shapefiles in `files/`, two to eight
years old, up to date from NYC Open Data. They are paused: only the landmark
shapes have clients today.

Production reads the GCS bucket `files-shapes`, not `files/`; every refresh is a
pull request that changes `files/`, and merging publishes and deploys it. CoreDataStore
uses three shapes (Individual Landmark Sites, Historic Districts, Scenic
Landmarks), looked up by LP number.

## Owner steps

| Item | Owner | Notes |
| --- | --- | --- |
| Projection for the NTA, borough and precinct shapes: (a) City Planning's own releases (same projection and fields; owner supplies the download links) or (b) Open Data exports in WGS 84, switching those shapes to `Datum.Wgs84` and reading codes as numbers | Owner | Blocks phase 1 |
| Missing features (phase 2): merge, or follow the city's file | Owner | Proposal: merge, keep the old feature for an LP number the city lacks, mark it as kept |
| One decision per shape in phase 3 | Owner | |
| Source of the National Register points (phase 4) | Owner | Not on NYC Open Data |
| Review this plan | Owner | |

## Inventory

Compared on 2026-11-01 against a fresh Open Data export
(`https://data.cityofnewyork.us/api/geospatial/<id>?method=export&format=Shapefile`).

| Shape (`ShapeProperties`) | Source | Rows (local → source) | Phase |
| --- | --- | --- | --- |
| `IndividualLandmarkHistoricDistricts` | `gpmc-yuvp` | 37,521 → 38,105 | 1 |
| `NeighborhoodTabulationAreas` | `9nt8-h7nd` | 262 → 262 | 1 |
| `BoroughBoundaries` | `gthc-hcne` | 5 → 5 | 1 |
| `NypdPolicePrecincts` | `y76i-bdw7` | 77 → 78 | 1 |
| `IndividualLandmarkSite` | `buis-pvji` (map view `ts56-fkf5`) | 1,520 → 1,532 | 2 |
| `HistoricDistricts` | `skyk-mpzq` (map view `xbvj-gfnw`) | 156 → 141 | 2 |
| `ScenicLandmarks` | `qexa-qpj6` (map view `gi7d-8gt5`) | 11 → 8 | 2 |
| `CommunityDistricts` | `5crt-au7u` | 71 → 71 | 2 |
| `DSNYDistricts` | `i6mn-amj2` | 59 → 59 | 2 |
| `NypdSectors` | `5rqd-h5ci` | 302 → 303 | 3 |
| `NychaDevelopments` | `phvi-damg` | 302 → 216 | 3 |
| `Parks` | `enfh-gkve` (Parks Properties) or `y6ja-fw4f` (planimetric) | 12,491 → 2,061 / 13,205 | 3 |
| `ZipCodes` | `35j5-n34v` (ZIP Code Tabulation Areas) | 263 → 221 | 3 |
| `Subway` | MTA `i9wp-a4ja` on data.ny.gov | 1,928 → 2,120 | 3 |
| `NationalRegisterHistoricPlaces` | not found | 6,417 | 4 |
| `Neighborhoods` (2010 NTAs) | retired; replaced by the 2020 NTAs | 195 | 4 |

## Next

| Item | Who | Blockers |
| --- | --- | --- |
| Phase 1: `scripts/refresh-shapes.py` (download by dataset id, rename to the configured base name, refuse to write if fields differ, print rows and keys added and removed), record each shape's id, URL, date and rows in `files/README.md`, refresh the four phase 1 shapes, with tests | Agent or Claude | The projection decision |
| Phase 2: mapping changes for renamed fields, merge in the missing features, add the new ones, tests | Agent or Claude | The missing-features decision |
| CoreDataStore landmark update plan (a separate document in CoreDataStore) | Claude | Phase 1 merged |

**Phase 1** (four shapes whose fields and features match today's, plus new rows).
Tests: each loads with the expected count; the 2020 NTA at 40.7158766, −73.9852317
is `MN0302`; the buildings data has LP-02692. Open Data writes whole-number codes
as decimals (`borocode` "1.0000…"). The buildings `.dbf` is over 100 MB, so
commit it as `<base>.dbf.zip` (the convention in the archive). The `bbl_int` and
`sc_flag` fields are gone from the buildings data; MicroService doesn't read them.

**Phase 2** (`IndividualLandmarkSite`, `HistoricDistricts`, `ScenicLandmarks`,
`CommunityDistricts`, `DSNYDistricts`; the first three draw every landmark outline
on CoreDataStore's map). Renamed fields, which the mapping profiles read:

| Shape | Old fields | New fields |
| --- | --- | --- |
| Individual Landmark Sites | `date_des_d`, `landmark_t`, `lpc_site_d`, `lpc_site_s` | `desdate`, `landmarkty`, `lpc_sitede`, `lpc_sitest` |
| Historic Districts, Scenic Landmarks | `desdate`, `caldate` | `date_desda`, `time_desda`, `date_calda`, `time_calda` |
| Community Districts | `borocd` | `boro_cd` |
| DSNY Districts | `fid`, `globalid` | `objectid` |

The LP number fields CoreDataStore looks up (`lpc_lpnumb`, `lp_number`) are
unchanged. Local features missing from the city's file: 12 Individual Landmark
Sites (including Brooklyn Bridge LP-00098, Rockefeller Center LP-01446, Queensboro
Bridge LP-00828), 15 Historic Districts (including Murray Hill LP-02102, Douglaston
Hill LP-02155; some are merges, such as Carnegie Hill into Expanded Carnegie Hill),
and 3 Scenic Landmarks (Ocean Parkway LP-00871, Eastern Parkway LP-00998,
Riverside Park LP-02002). New in the city's file: 13 landmark sites (2023 to April
2026) and 5 districts (including Willoughby-Hart, Beverley Square West, Ditmas
Park West). Until this phase refreshes the file, add each new district with
`scripts/add-historic-districts.py`.

## Later

- **Phase 3, shapes the city redefined** (none used by CoreDataStore): NYPD
  Sectors (new `nco_phase`, `sector_ind`; `phase`, `sct_text` and others gone);
  NYCHA Developments (302 → 216); Parks (Parks Properties `enfh-gkve` is the
  better source for "which park is this in"); ZIP codes (only the Census
  tabulation areas remain, a different geography); Subway Entrances (MTA feed,
  2,120 entrances). `Subway_Stations/` is in git, unread by any shape, and goes
  with Subway Entrances.
- **Phase 4, no current source:** National Register points (owner to say where
  they came from); Neighborhoods (2010 NTAs: keep for existing callers or remove).
- **Vector DB context.** Refreshing all shapes gives nyc-landmarks-vector-db the
  borough, district, NTA, precinct, park, ZIP and subway context for each landmark.
- **Downstream.** CoreDataStore's `LPCReport` (LPC's `landmarks.js`, 13 designations
  from 2025 to 2026 missing), `lpc_designated_sites` (`ncre-qhxs`), `LPCLocation`
  (PLUTO 26v2, 2020 NTAs), photos and PDFs, and the map outline (phase 2) depend
  on phases 1 and 2.

## Validation for every pull request

- `make check`, `make build` and `make test` pass; CI passes on the head commit.
- The refresh summary for each shape is in the pull request: rows before and
  after, keys added and removed.
- The Copilot review is complete, and every thread is answered and resolved.
- After a production upload: the new revision serves 100% of traffic, each
  refreshed shape answers a known lookup, and CoreDataStore's landmark pages
  still show their outlines.
