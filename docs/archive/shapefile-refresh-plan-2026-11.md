# Shapefile refresh plan: finished work

Finished phases moved out of [the shapefile refresh plan](../shapefile-refresh-plan.md) on 2026-11-11, unchanged.

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
7. The deploy runs as today, then `scripts/shape-smoke-check.sh` runs one
   known lookup for each shape a client uses (the four landmark shapes, borough
   boundaries and Subway Entrances).

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

`Subway_Stations/` is in git and, since the first publish, in the bucket; no
shape reads it. It goes into phase 3 with Subway Entrances.

**Done when:** a pull request that changes only `files/README.md` publishes,
deploys and passes the smoke check, and the bucket matches git.

## Found during the review (fixed)

- **Subway Entrances projection (fixed in #554):** the file's `.prj` is WGS 84,
  but `ShapeProperties` configured it as `Datum.Nad83`, and the lookup treated
  every request as NAD83. A WGS 84 lookup at Lexington Ave & 68th St returned a
  Staten Island Railway entrance. The shape is now `Wgs84` and the lookup
  honours the request's datum.
- **Historic district outlines (fixed 2026-11-02):** three districts in
  CoreDataStore had no map. LP-02683 (Willoughby-Hart) was missing from the
  WGS84 file, and LP-02607 (Central Harlem) and LP-02665 (Linden Street) were
  stored as `LP-2607` and `LP-2665`, so the exact `lp_number` lookup the UI uses
  found nothing. `scripts/add-historic-districts.py` replaced those two with
  LPC's current outlines under padded numbers and added LP-02683 and the two
  2025 designations staged in CoreDataStore (LP-02692, LP-02693), from Open
  Data `skyk-mpzq`. 159 districts; the other 154 records are unchanged. Use the
  same script for each new district until phase 2 refreshes the file.
