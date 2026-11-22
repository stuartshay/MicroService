# Map Layers Plan

<!-- cspell:ignore Greenstreets gthc hcne skyk mpzq qexa qpj6 rvih nhyn mk9u qu7i feuq bdha 6eqy xbvj gfnw gi7d 8gt5 qeta 4kqg vzj6 pcjy p4pf fyc4 gpxw bq7a yqww -->

The owner asked on 2026-11-22 for seven NYC Open Data layers in MicroService, so that a
navigatorglass.com map can show them. Three are refreshed and four are new. Each map picks
its own layers, and the maps site draws them within the map's area, with a toggle per layer.

## Decisions (owner, 2026-11-22)

| Decision | Answer |
| --- | --- |
| Historic Districts and Scenic Landmarks the city's file no longer has | Keep them, marked as kept, so CoreDataStore's LP number lookups still find an outline. Take the city's outlines and the new districts. |
| How a map uses layers | Each map stores its layers. The maps site draws each one within the map's area, with its own toggle (like subway and tax lots), and a click shows the feature's name and details. |
| Library (2016) and Post Office (2014) are years old | Use them, and show the data year in the layer's legend |

## Layers

The links the owner gave are map views. Each layer is read from the dataset behind its view,
exported as a WGS 84 shapefile
(`https://data.cityofnewyork.us/api/geospatial/<id>?method=export&format=Shapefile`).

| Layer key | Dataset (map view) | Geometry | Rows (2026-11-22) | Data updated | Status |
| --- | --- | --- | --- | --- | --- |
| `BoroughBoundaries` | `gthc-hcne` (`yqww-f9f3`) | Polygons | 5 | 2026-11-02 | Refresh: switch to the WGS 84 export |
| `HistoricDistricts` | `skyk-mpzq` (`xbvj-gfnw`) | Polygons | 141 (156 in `files/`) | 2026-06-18 | Refresh: keep the 15 dropped |
| `ScenicLandmarks` | `qexa-qpj6` (`gi7d-8gt5`) | Polygons | 8 (11 in `files/`) | 2026-06-18 | Refresh: keep the 3 dropped |
| `PrivatelyOwnedPublicSpaces` | `rvih-nhyn` (`qeta-4kqg`) | Points (building) | 392 | 2026-11-05 | New |
| `Greenstreets` | `mk9u-qu7i` (`vzj6-pcjy`) | Polygons | 2,755 | 2026-11-02 | New |
| `Libraries` | `feuq-due4` (`p4pf-fyc4`) | Points | 216 | 2016-06-22 | New |
| `PostOffices` | `bdha-6eqy` (`gpxw-bq7a`) | Points | 230 | 2014-11-18 | New |

The refreshes are the borough, Historic Districts and Scenic Landmarks parts of the
[shapefile refresh plan](shapefile-refresh-plan.md)'s phases 1 and 2. These decisions settle
that plan's projection question for the borough shape (b: the WGS 84 export) and its
missing-features question (merge).

## Phases

| Phase | Repository | Work | Status |
| --- | --- | --- | --- |
| 1 | MicroService | `scripts/refresh-shapes.py` (export by id; keep the dropped features of a shape that asks for it); refresh the three shapes, with the field renames the refresh plan lists; add the four new shapes (models, mapping profiles, services, registry, tests); `files/README.md` | ![Next](https://img.shields.io/badge/Next-blue) |
| 2 | MicroService | `GET /api/v1/Layers`: each layer's key, name, geometry, color, source and data date. `GET /api/v1/Layers/{key}?bbox=west,south,east,north`: the features in the box as GeoJSON, with a few display fields; polygons simplified; at most 2,000 features; cached for a day | ![Proposed](https://img.shields.io/badge/Proposed-lightgrey) |
| 3 | NavigatorMapsAPI | Map option `layers: ["HistoricDistricts", ...]`, which works like `subway` (left out keeps it, null removes it); keys checked against MicroService's list | ![Proposed](https://img.shields.io/badge/Proposed-lightgrey) |
| 4 | NavigatorMaps.UI | Draw a map's layers within the places' bounds plus 1 km: polygons outlined and lightly filled in the layer's color, points as small dots, a toggle per layer with the data year, and an info window on click | ![Proposed](https://img.shields.io/badge/Proposed-lightgrey) |
| 5 | Navigator.UI | Layers section on the map edit page (a checkbox per layer), drawn on the preview | ![Proposed](https://img.shields.io/badge/Proposed-lightgrey) |
| 6 | Navigator.MCP | `create_map` and `update_map` take `layers` | ![Proposed](https://img.shields.io/badge/Proposed-lightgrey) |

Merging to `master` publishes `files/` to the production bucket and deploys, so phases 1
and 2 are production changes the owner merges. Phases 3 to 6 follow the street lines
pattern: MicroService #587, NavigatorMapsAPI #136, NavigatorMaps.UI #334, Navigator.UI #373
and Navigator.MCP #36.

## Validation

- `make build`, `make analyze` and `make test` pass; CI and CodeFactor pass on the head
  commit.
- For each refreshed shape, the pull request lists rows before and after, keys added and
  removed, and the kept features.
- CoreDataStore still finds the outline of every Historic District and Scenic Landmark it
  found before, including the kept ones (Murray Hill LP-02102, Ocean Parkway LP-00871).
