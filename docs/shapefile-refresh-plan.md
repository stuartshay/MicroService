# Shapefile Refresh Plan

<!-- cspell:ignore Ditmas Douglaston HRHP MODZCTA Queensboro Willoughby ZCTA ZCTAs borocd borocode calda damg desda dsny enfh gfnw gkve gpmc gthc hcne landmarkty lpnumb mpzq ncre nybb nycd nynta nypp phvi planimetric pvji qexa qhxs sitede sitest skyk xbvj yuvp -->

Status: phase 0 live (2026-11-01, #551, revision `microservice-api-00169-tnz`).
The first publish matched the dry run (6 added, 19 replaced, 28 removed) and the
bucket matches git, but rsync also deleted that run's 47 backups. They are in soft
delete until 2026-11-08; the publish fix keeps backups out of the deletions. The
other phases are drafts for the owner's review.

The shapefiles in `files/` are two to eight years old. The landmark ones stop at
December 2022, so the 2023–2026 designations have no outline on CoreDataStore's
map. This plan brings them up to date from NYC Open Data, one group at a time,
starting with the shapes that are the same as today's in every way except age.
Git stays the source of truth: phase 0 makes a merge to `master` publish
`files/` to the production bucket, so every later phase is a pull request.

It is the first step of a larger piece of work: keeping CoreDataStore's
landmarks up to date as LPC designates new ones (3 to 5 a year). That work has
its own plan in CoreDataStore, which depends on this one (see
[Downstream](#downstream-coredatastore-landmark-updates)).

## How shapes reach production

- `files/` in this repository holds the shapefiles and the sources in
  `files/README.md`.
- Production does not read `files/`. Cloud Run mounts the GCS bucket
  `files-shapes` read-only at `/mnt/data/shapes`. On 2026-11-01 the bucket
  matched the repository (same sizes); it was last uploaded on 2024-02-06 and
  2024-02-11 (151 objects, 537 MiB).
- The bucket has no object versioning, only the 7-day soft delete, so an upload
  that replaces a file must keep a copy of the old one.
- Uploads have been by hand. Phase 0 replaces that with the deploy workflow.
- The service caches loaded shapefiles in memory (`CachedShapefileDataReader`),
  so a new revision is needed to serve replaced files.
- CoreDataStore uses three shapes today: Individual Landmark Sites, Historic
  Districts and Scenic Landmarks, looked up by LP number through
  `POST /api/v1/FeatureService/attributelookup`. MicroService serves the rest,
  but no client calls them.

## Inventory

Compared on 2026-11-01: each local `.dbf` against a fresh Open Data export
(`https://data.cityofnewyork.us/api/geospatial/<id>?method=export&format=Shapefile`).

| Shape (`ShapeProperties`) | Local file | Source | Source updated | Rows (local → source) | Group |
| --- | --- | --- | --- | --- | --- |
| `IndividualLandmarkHistoricDistricts` | `LPC_Individual_Landmark_and_Historic_Building_Database/…_20231209` | `gpmc-yuvp` | 2026-02-03 | 37,521 → 38,105 | 1 |
| `NeighborhoodTabulationAreas` | `Neighborhood_Tabulation_Areas/nynta2020` | `9nt8-h7nd` | 2026-05-28 | 262 → 262 | 1 |
| `BoroughBoundaries` | `Borough_Boundaries/NAD83/nybb` | `gthc-hcne` | 2026-05-26 | 5 → 5 | 1 |
| `NypdPolicePrecincts` | `NYPD_Police_Precincts/nypp` | `y76i-bdw7` | 2026-05-26 | 77 → 78 | 1 |
| `IndividualLandmarkSite` | `Individual_Landmark_Sites/Individual_Landmark_Sites` | `buis-pvji` (map view `ts56-fkf5`) | 2026-06-30 | 1,520 → 1,532 | 2 |
| `HistoricDistricts` | `Historic_Districts/WGS84/Historic_Districts` | `skyk-mpzq` (map view `xbvj-gfnw`) | 2026-06-18 | 156 → 141 | 2 |
| `ScenicLandmarks` | `Scenic_Landmarks/WGS84/Scenic_Landmarks` | `qexa-qpj6` (map view `gi7d-8gt5`) | 2026-06-18 | 11 → 8 | 2 |
| `CommunityDistricts` | `Community_Districts/nycd` | `5crt-au7u` | 2026-05-26 | 71 → 71 | 2 |
| `DSNYDistricts` | `DSNY_Districts/WGS84/dsny` | `i6mn-amj2` | 2026-09-17 | 59 → 59 | 2 |
| `NypdSectors` | `NYPD_Sectors/NYPD_Sectors` | `5rqd-h5ci` | 2025-01-27 | 302 → 303 | 3 |
| `NychaDevelopments` | `NYCHA_Developments/NYCHA_Developments` | `phvi-damg` | 2026-10-05 | 302 → 216 | 3 |
| `Parks` | `Park/PARK` | `enfh-gkve` (Parks Properties) or `y6ja-fw4f` (planimetric open space) | 2026-10-16 / 2024-04-25 | 12,491 → 2,061 / 13,205 | 3 |
| `ZipCodes` | `Zip_Codes/ZIP_CODE_040114` | `35j5-n34v` (ZIP Code Tabulation Areas) | 2026-04-10 | 263 → 221 | 3 |
| `Subway` | `Subway_Entrances/Subway_Entrances` | MTA `i9wp-a4ja` on data.ny.gov | 2025-12-05 | 1,928 → 2,120 | 3 |
| `NationalRegisterHistoricPlaces` | `National_Register_ Historic_Places/WGS84/HRHP` | not found on NYC Open Data | — | 6,417 | 4 |
| `Neighborhoods` | `Neighborhoods/nynta` (2010 NTAs) | no longer published; replaced by the 2020 NTAs | — | 195 | 4 |

Groups:

1. **Same shape, newer version.** The same fields and the same features, plus new
   rows. Phase 1.
2. **A field renamed, or features missing.** Needs mapping changes and a
   decision on missing features. Phase 2.
3. **Redefined by the city.** Different fields, a different dataset or a
   different geography. Phase 3, one decision each.
4. **No current source found.** Phase 4.

## Phase 0: publish shapes from git

Owner decision (2026-11-01): keep the shapefiles in git, and have a merge to
`master` copy them to the bucket. No Cloud Function: the existing deploy
workflow does it, because:

- a pull request can change files and code together (phase 2 changes both),
  and the upload must land before the new revision starts. One workflow with
  the upload job ahead of the deploy job guarantees that;
- the deploy creates a new revision, which reloads the shape cache;
- the deployer account and workload identity already exist;
- the pull request's tests run against the new files, so a bad shapefile fails
  CI before it reaches production.

A function triggered by uploads would run after the file is already live and
would still need to restart Cloud Run. Processing (download, changing the projection,
merging) happens in the refresh script, in the pull request, so what git holds
is exactly what production serves.

### Large files

GitHub rejects files over 100 MB. Today the one file that large, the 271 MB
building database `.dbf`, is git-ignored, and git holds the raw Open Data export
(`20231209/Individual Landmark and Historic District Building Database_20240211.zip`,
13 MB, with `geo_export_…` names) that someone renamed by hand. The current
export's `.dbf` is 282 MB.

Convention: a `.dbf` over 100 MB is committed as `<base>.dbf.zip`, holding only
`<base>.dbf`, next to the shape's other parts. CI and the publish job unzip it;
`.gitignore` keeps the unzipped `.dbf` out of git.

### Publishing

`actions.yml` gains `files/**` in its paths and a `publish-shapes` job that runs
before `deploy-googlecloud` when `files/` changed:

1. Unzip every `*.dbf.zip` in place.
2. Sign in with workload identity as the deployer.
3. List the changes: `gcloud storage rsync --dry-run --checksums-only
   --delete-unmatched-destination-objects files/ gs://files-shapes`, then drop
   `_previous/` paths from the deletions. An `--exclude` filters only the source,
   so it does not protect bucket objects from deletion.
4. Back up every object that will be replaced or deleted to
   `gs://files-shapes/_previous/<commit>/`.
5. Guard: stop if the change would delete a file a shape in `ShapeProperties`
   reads, or more than 20 objects, unless the run was started by hand with
   `allow_deletions`.
6. Run the sync without `--delete-unmatched-destination-objects`, then delete the
   planned removals with `gcloud storage rm`. The first publish (2026-11-01) let
   rsync delete and lost its own 47 backups to soft delete; see the status.
7. The deploy runs as today, then a smoke check: one known lookup per changed
   shape.

Deletions are mirrored, so the bucket always matches git; the backup and the
guard make that safe. A lifecycle rule deletes `_previous/` objects after 90
days. The bucket has no versioning, so `_previous/` is the rollback: copy the
objects back and deploy a new revision.

The deployer needs object admin on `files-shapes` only (not project-wide):
a Terraform change in nyc-landmarks-vector-db, prepared by Claude and applied
with the owner's approval, as for the other deploy accounts.

### Clean-up that goes with it

Nothing reads these, so they are removed from git; the first publish mirrors
the removal and keeps a copy in `_previous/`:

- `Individual_Landmark_Sites/Archive/`, `Neighborhoods/Archive/`,
  `LPC_Individual_Landmark_and_Historic_Building_Database/Archive/` and
  `LPC_Individual_Landmark_and_Historic_Building_Database/202302/` (about
  165 MB of older versions);
- in the bucket only: a second copy of the `202302` files at the folder's top
  level, and a Windows shortcut
  (`LPC_Individual_Landmark_and_Historic_Building_Database - Shortcut.lnk`);
- the `20231209/` raw export zip, once its `.dbf` follows the convention above
  (phase 1 replaces it with the 2026 export anyway).

`Subway_Stations/` is in git but not in the bucket, and no shape reads it. It
goes into phase 3 with Subway Entrances.

**Done when:** a pull request that changes only `files/README.md` publishes,
deploys and passes the smoke check, and the bucket matches git.

## Phase 1: the four 1:1 shapes

**Scope:** `IndividualLandmarkHistoricDistricts`, `NeighborhoodTabulationAreas`,
`BoroughBoundaries` and `NypdPolicePrecincts`.

What changes in the data:

- **Landmark & District Buildings** (`gpmc-yuvp`): 584 more buildings. Two
  fields MicroService doesn't read, `bbl_int` and `sc_flag`, are gone. Same
  projection (WGS 84).
- **2020 NTAs** (`9nt8-h7nd`): the same 11 fields and the same 262 areas.
- **Borough Boundaries** (`gthc-hcne`): the same fields and 5 boroughs.
- **Police Precincts** (`y76i-bdw7`): the same fields, plus one new precinct.

**Decision needed (owner): projection.** The local NTA, borough and precinct
files are in NAD83 State Plane (`NAD_1983_StatePlane_New_York_Long_Island_FIPS_3104_Feet`),
City Planning's format, and `ShapeProperties` says `Datum.Nad83` for them. Open
Data exports only WGS 84, and writes whole-number codes as decimals (`borocode`
"1" becomes "1.0000…", `precinct` the same). Either:

- **(a)** download City Planning's own releases (`nybb`, `nynta2020`, `nypp`),
  which have the same projection and fields as the files we have. City
  Planning's download pages render in the browser, so the owner supplies the
  links; or
- **(b)** use the Open Data export, switch those three shapes to `Datum.Wgs84`,
  and read the codes as numbers. MicroService already serves WGS 84 shapes.

**Work:**

1. A refresh script, `scripts/refresh-shapes.py`. For each shape it is given:
   - it downloads the source by its dataset id;
   - it unzips into a temporary folder and renames the files to the configured
     base name (`nynta2020.shp`, …);
   - it compares field names and types with the current file and refuses to
     write if they differ, unless told the difference is expected;
   - it writes the files into `files/<folder>/` and prints a summary: rows
     before and after, LP numbers or codes added and removed.
2. `files/README.md` records each shape's dataset id, export URL, download date
   and row count, so the next refresh is a single command.
3. Tests for each refreshed shape:
   - it loads and has the expected number of features;
   - a known lookup works, for example the 2020 NTA for the Church of Saint
     Mary point (40.7158766, −73.9852317) is `MN0302`, and the buildings data has
     rows for LP-02692 (Beverley Square West, November 2025).
4. Merging publishes them through phase 0: the replaced files go to
   `_previous/<commit>/`, the deploy reloads the cache, and the smoke check
   covers each refreshed shape.

**Done when:** the four shapes in production return the 2026 data, every other
shape is unchanged, and the old files are kept in `_previous/<commit>/`.

## Phase 2: renamed fields and missing features

**Scope:** `IndividualLandmarkSite`, `HistoricDistricts`, `ScenicLandmarks`,
`CommunityDistricts` and `DSNYDistricts`. The first three matter most: they draw
the outline of every landmark on CoreDataStore's map.

**Renamed fields** (the mapping profiles read the old names):

| Shape | Old field | New field |
| --- | --- | --- |
| Individual Landmark Sites | `date_des_d`, `landmark_t`, `lpc_site_d`, `lpc_site_s` | `desdate`, `landmarkty`, `lpc_sitede`, `lpc_sitest` (also new: `latitude`, `longitude`, `nta2020`, `cd`, `council`, `bct2020`) |
| Historic Districts, Scenic Landmarks | `desdate`, `caldate` | `date_desda` and `time_desda`, `date_calda` and `time_calda` |
| Community Districts | `borocd` | `boro_cd` |
| DSNY Districts | `fid`, `globalid` | `objectid` |

The LP number fields CoreDataStore looks up by (`lpc_lpnumb`, `lp_number`) are
unchanged.

**Features in the local file but missing from the city's** (decision needed
for each):

- **Individual Landmark Sites:** 12 LP numbers, including Brooklyn Bridge
  (LP-00098), Rockefeller Center (LP-01446), Queensboro Bridge (LP-00828), High
  Bridge (LP-00639), Harlem River Houses (LP-00894) and Williamsburg Houses
  (LP-02135, LP-02135A). These are still designated: Brooklyn Bridge is in LPC's
  list and in `ncre-qhxs`. They are only absent from this feed.
- **Historic Districts:** 15, including Murray Hill (LP-02102) and its
  extensions (LP-02140), Douglaston Hill (LP-02155), the Park Slope extensions
  (LP-02443, LP-02558) and East Village/Lower East Side (LP-02491). Some are
  merges: Carnegie Hill (LP-00861) is now covered by Expanded Carnegie Hill
  (LP-01834).
- **Scenic Landmarks:** 3: Ocean Parkway (LP-00871), Eastern Parkway (LP-00998),
  and Riverside Park and Riverside Drive (LP-02002).

Proposal: merge rather than replace. Take every feature from the city's file,
keep the old feature for an LP number the city's file lacks, mark it as kept,
and list it in the refresh summary.

**New features:**

- **Individual Landmark Sites:** 13 sites from 2023 to April 2026.
- **Historic Districts:** 5, including Willoughby-Hart (2024), Beverley Square
  West and Ditmas Park West (November 2025).

## Phase 3: shapes the city redefined

One decision each; none of them is used by CoreDataStore today.

- **NYPD Sectors:** the new fields are `nco_phase` and `sector_ind`; `phase`,
  `sct_text`, `sq_feet`, `start_date` and others are gone.
- **NYCHA Developments:** 302 → 216 developments; the city's definition changed.
- **Parks:** today's file is a planimetric layer (12,491 polygons). Parks
  Properties (`enfh-gkve`, 2,061 properties with names, acres and jurisdiction)
  is the better source for "which park is this in"; `y6ja-fw4f` is the
  planimetric successor.
- **ZIP codes:** the local 2014 ZIP boundary file has no successor on Open Data;
  the closest is the Census ZIP Code Tabulation Areas (`35j5-n34v`), a
  different geography. CoreDataStore takes ZIP codes from PLUTO, so this only
  affects MicroService's own ZIP shape.
- **Subway Entrances:** the MTA's feed on data.ny.gov (2,120 entrances, with
  station and route fields).

## Phase 4: shapes without a current source

- **National Register of Historic Places** (6,417 points): the source is not on
  NYC Open Data; the owner to say where it came from (New York State or the
  National Park Service).
- **Neighborhoods (2010 NTAs):** retired by City Planning. Keep it for existing
  callers, or remove it once nothing needs it.

## Found during the review

- **Subway Entrances projection:** the file's `.prj` is WGS 84, but
  `ShapeProperties` configures it as `Datum.Nad83`. If it is served, the points
  may be transformed into the wrong place. Phase 1 adds a test; the fix goes
  with phase 3.
- **Encoding:** the exports are UTF-8 with a `.cpg` file, which
  `ShapefileSource` already honours. Without it, a name like "Central Harlem –
  West 130th-132nd Streets Historic District" would be misread.
- **Decimals for codes:** see the projection decision in phase 1.

## Shapes and the vector DB

Most shapes have no client today. For nyc-landmarks-vector-db they become
context for each landmark: its borough, community district, 2020 NTA, police
precinct, DSNY district, park, ZIP code and nearby subway entrances. That is a
reason to refresh them all, even the unused ones, and to keep
`files/README.md` complete.

## Downstream: CoreDataStore landmark updates

Covered by its own plan in CoreDataStore; summarized here because it depends on
phases 1 and 2.

| CoreDataStore data | Source |
| --- | --- |
| `LPCReport` | LPC's `https://www.nyc.gov/assets/lpc/js/landmarks.js`, the data behind the designation reports page (1,774 designations; 13 from 2025–2026 are not in CoreDataStore) |
| `lpc_designated_sites` | `ncre-qhxs`, Designated and Calendared Buildings and Sites (the same columns) |
| `LPCLocation` | BBL from the landmark sites data; latitude, longitude and ZIP from PLUTO (CoreDataStore has 24v4; current is 26v2); neighborhood from the 2020 NTAs; map zoom per the UI |
| Photo and PDF | `https://www.nyc.gov/assets/lpc/images/content/designations/<n>.jpg` and `https://s-media.nyc.gov/agencies/lpc/lp/<n>.pdf`, copied to the Azure container behind `cdn.informationcart.com` |
| Map outline | this plan's phase 2 |

## Next work and owners

**Agent**: repository-only work in one pull request that stops for review.
**Claude**: production, Cloud Run, GCS and deploy work, and reviewing agent
pull requests. **Owner**: decisions and sources.

| Item | Owner | Depends on | Notes |
| --- | --- | --- | --- |
| Review this plan and add details | Owner | — | |
| Phase 0: `publish-shapes` job, large-file convention, clean-up | Claude | — | Merging deploys |
| Phase 0: deployer gets object admin on `files-shapes` (Terraform) | Claude (+ owner applies) | — | nyc-landmarks-vector-db |
| Projection for the three NAD83 shapes: City Planning releases (a) or WGS 84 exports (b) | Owner | — | (a) needs City Planning's download links |
| Phase 1: refresh script, `files/README.md`, the four shapes and their tests | Agent or Claude | Phase 0 and the projection decision | Merging publishes and deploys |
| Phase 2: missing features, merge or follow the city | Owner | — | Proposal: merge and mark as kept |
| Phase 2: mapping changes, merge, tests | Agent or Claude | The decision above | CoreDataStore's three shapes |
| Phase 3: one decision per shape | Owner | — | |
| Phase 4: the National Register source | Owner | — | |
| CoreDataStore landmark update plan | Claude | Phase 1 merged | Separate document in CoreDataStore |

## Validation for every pull request

- `make check`, `make build` and `make test` pass; CI passes on the head commit.
- The refresh summary for each shape is in the pull request: rows before and
  after, keys added and removed.
- The Copilot review is complete, and every thread is answered and resolved.
- After a production upload: the new revision serves 100% of traffic, each
  refreshed shape answers a known lookup, and CoreDataStore's landmark pages
  still show their outlines.
