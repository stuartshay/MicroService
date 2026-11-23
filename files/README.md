# Data Files

<!-- cspell:ignore Greenstreets rvih nhyn mk9u qu7i feuq bdha 6eqy qeta 4kqg vzj6 pcjy p4pf fyc4 gpxw bq7a yqww f9f3 irgq gpmc yuvp gthc hcne buis pvji fkf5 skyk mpzq xbvj gfnw qexa qpj6 gi7d 8gt5 5crt au7u i6mn amj2 5rqd h5ci phvi damg enfh gkve y6ja fw4f 35j5 n34v i9wp a4ja 9nt8 h7nd y76i bdw7 nybb nycd dsny nynta nypp -->

The datasets the API serves, versioned with the code. `files/` is the source of
truth: merging a change here publishes it to the `files-shapes` bucket before the
deploy, and Cloud Run reads the bucket (mounted read-only at `/mnt/data/shapes`).
See "Datasets" in the [README](../README.md) for how publishing works, and the
[shapefile refresh plan](../docs/shapefile-refresh-plan.md) for bringing them up to
date. A `.dbf` over GitHub's 100 MB limit is committed as `<name>.dbf.zip`.

## Shapes

Served by `/api/v1/FeatureService` under the key in the first column. Sources are
NYC Open Data dataset ids (`https://data.cityofnewyork.us/d/<id>`), as recorded
in the refresh plan's inventory on 2026-11-01.

| Key | Folder and file | Datum | Source |
| --- | --- | --- | --- |
| `BoroughBoundaries` | `Borough_Boundaries/WGS84/nybb` | WGS84 | `gthc-hcne` (map view `yqww-f9f3`), refreshed 2026-11-22 |
| `CommunityDistricts` | `Community_Districts/nycd` | NAD83 | `5crt-au7u` |
| `DSNYDistricts` | `DSNY_Districts/WGS84/dsny` | WGS84 | `i6mn-amj2` |
| `HistoricDistricts` | `Historic_Districts/WGS84/Historic_Districts` | WGS84 | `skyk-mpzq` (map view `xbvj-gfnw`), refreshed 2026-11-22: 141 from the city and 18 kept |
| `IndividualLandmarkSite` | `Individual_Landmark_Sites/Individual_Landmark_Sites` | WGS84 | `buis-pvji` (map view `ts56-fkf5`) |
| `IndividualLandmarkHistoricDistricts` | `LPC_Individual_Landmark_and_Historic_Building_Database/…_20231209` | WGS84 | `gpmc-yuvp` |
| `NationalRegisterHistoricPlaces` | `National_Register_ Historic_Places/WGS84/HRHP` | WGS84 | Not on NYC Open Data; origin unknown |
| `Neighborhoods` | `Neighborhoods/nynta` | NAD83 | 2010 Neighborhood Tabulation Areas, retired |
| `NeighborhoodTabulationAreas` | `Neighborhood_Tabulation_Areas/nynta2020` | NAD83 | `9nt8-h7nd` |
| `NypdPolicePrecincts` | `NYPD_Police_Precincts/nypp` | NAD83 | `y76i-bdw7` |
| `NypdSectors` | `NYPD_Sectors/NYPD_Sectors` | NAD83 | `5rqd-h5ci` |
| `NychaDevelopments` | `NYCHA_Developments/NYCHA_Developments` | NAD83 | `phvi-damg` |
| `Parks` | `Park/PARK` | NAD83 | `enfh-gkve` (Parks Properties) or `y6ja-fw4f` (planimetric open space) |
| `ScenicLandmarks` | `Scenic_Landmarks/WGS84/Scenic_Landmarks` | WGS84 | `qexa-qpj6` (map view `gi7d-8gt5`), refreshed 2026-11-22: 8 from the city and 3 kept |
| `Subway` | `Subway_Entrances/Subway_Entrances` | WGS84 | MTA `i9wp-a4ja` on data.ny.gov |
| `SubwayServiceLines` | `Subway_Service_Lines/Subway_Service_Lines` | WGS84 | MTA `s692-irgq` on data.ny.gov (Subway Service Lines, exported 2026-11-20) |
| `ZipCodes` | `Zip_Codes/ZIP_CODE_040114` | NAD83 | 2014 ZIP boundaries; Open Data now has only ZIP Code Tabulation Areas (`35j5-n34v`) |
| `PrivatelyOwnedPublicSpaces` | `Privately_Owned_Public_Spaces/WGS84/POPS` | WGS84 | `rvih-nhyn` (map view `qeta-4kqg`), 392 building points, added 2026-11-22 |
| `Greenstreets` | `Greenstreets/WGS84/Greenstreets` | WGS84 | `mk9u-qu7i` (map view `vzj6-pcjy`), 2,755, added 2026-11-22 |
| `Libraries` | `Libraries/WGS84/Libraries` | WGS84 | `feuq-due4` (map view `p4pf-fyc4`), 216, data last updated 2016, added 2026-11-22 |
| `PostOffices` | `Post_Offices/WGS84/Post_Offices` | WGS84 | `bdha-6eqy` (map view `gpxw-bq7a`), 230, data last updated 2014, added 2026-11-22 |

`scripts/refresh-shapes.py` refreshes the borough, Historic Districts, Scenic Landmarks and
the four map layer shapes from their Open Data exports (`--check` compares without writing),
for the [map layers plan](../docs/archive/map-layers-plan-2026-11.md). Historic Districts and Scenic
Landmarks export in NY State Plane feet although their `.prj` says WGS 84, so the script
reprojects them; it keeps the features the city's file no longer has (owner decision,
2026-11-22), marked `kept` = `Yes`, so CoreDataStore's LP number lookups still find them.
Text is UTF-8 (each written file has a `.cpg`).

The 2014 ZIP file isn't refreshed, so codes the Postal Service has since retired are
replaced in its `.dbf` by `scripts/fix-zip-codes.py` (the outline stays): 10048, the
World Trade Center's own code, is 10007. `python3 scripts/fix-zip-codes.py --check`
fails if one is left. `00083` (Central Park) is the city's code for the park, not a
postal one; clients that need a city use the borough's.

The `IndividualLandmarkHistoricDistricts` `.dbf` is committed as a zip; see
[its folder](LPC_Individual_Landmark_and_Historic_Building_Database/README.md).
`Projections/` holds the NAD83 State Plane (Long Island) projection the
conversions use.

## Flat files

Served by `/api/v1/FlatFileService/{id}`; `/api/v1/Subway/stations` also reads
`Stations.csv`.

| Id | Folder and file | Source |
| --- | --- | --- |
| `SubwayStationLocations` | `Subway_Station_Locations/Stations.csv` | MTA Subway Stations, data.ny.gov `39hk-dx4f` (updated 2026-11-19, downloaded 2026-11-20, rows sorted by Station ID) |
| `SubwayStationComplex` | `Subway_Complex/StationComplexes.csv` | MTA station complexes; source not recorded |

## Not served

Nothing in the code reads these folders, but the publish job copies them to the
bucket with the rest. Whether to keep them as reference data or remove them is an
open owner decision in the [improvement plan](../docs/improvement-plan.md).

| Folder | Contents |
| --- | --- |
| `Borough_Boundaries/NAD83/` | The NAD83 borough release `BoroughBoundaries` read until 2026-11-22 |
| `Fire Companies/` | Fire company boundaries (`nyfc_20c.zip`, a zipped shapefile) |
| `Historical_Signs/` | NYC Parks historical sign text (JSON) |
| `Monuments/` | `Monuments.xlsx` |
| `NYC_Parks_Monuments/` | NYC Parks monuments (`NYC_Parks_Monuments.csv`) |
| `Parking Meters/` | Parking meter locations (`dot_MUNIMETER_20201130.csv` and the zipped export) |
| `Points Of Interest/` | Points of interest (`Point_Of_Interest.zip`, a zipped shapefile) |
| `Subway_Stations/` | Subway station points; goes with `Subway` in the refresh plan's phase 3 |
| `reference/` | `community_districts.json` |
