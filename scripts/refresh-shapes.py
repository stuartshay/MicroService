#!/usr/bin/env python3
"""Refresh shapes in files/ from their NYC Open Data datasets, and add new ones.

Each shape is exported from its dataset as a WGS 84 shapefile
(https://data.cityofnewyork.us/api/geospatial/<id>?method=export&format=Shapefile) and
written to the shape's folder under its configured base name, with a UTF-8 .cpg. Some
exports are in NY State Plane feet (EPSG:2263) although their .prj says WGS 84; a shape
that names its source projection is reprojected to WGS 84 (lon/lat). A shape
can rename and convert the export's fields to the ones its mapping profile reads, and can
keep the features the city's file no longer has (matched by a key field, marked kept=Yes),
so lookups by that key still find them (owner decision, docs/map-layers-plan.md).

    pip install pyshp==2.3.1 pyproj==3.6.1
    python3 scripts/refresh-shapes.py --check                    # compare every shape, write nothing
    python3 scripts/refresh-shapes.py HistoricDistricts Libraries

For each shape it prints the rows before and after and the keys added, dropped and kept.
"""

from __future__ import annotations

import argparse
import http.client
import io
import sys
import tempfile
import zipfile
from dataclasses import dataclass, field
from datetime import date
from pathlib import Path
from typing import Callable

import shapefile
from pyproj import Transformer

# Fetched over HTTPS from this host only
DATASET_HOST = "data.cityofnewyork.us"
EXPORT_PATH = "/api/geospatial/{id}?method=export&format=Shapefile"
USER_AGENT = "MicroService-refresh-shapes (+https://github.com/stuartshay/MicroService)"
FILES = Path(__file__).resolve().parent.parent / "files"
KEPT_FIELD = ["kept", "C", 3, 0]


def us_date(value) -> str:
    """A dbf date as the M/D/YYYY text the landmark shapes have always stored."""
    if isinstance(value, date):
        return f"{value.month}/{value.day}/{value.year}"
    return ""


def yes_no(value) -> str:
    """The export's true/false as the Yes/No the landmark shapes store; other text as it is."""
    text = str(value or "").strip()
    return {"true": "Yes", "false": "No"}.get(text.lower(), text)


def number(value) -> float | None:
    """The export writes some numbers as text."""
    try:
        return float(value)
    except (TypeError, ValueError):
        return None


def whole(value) -> int | None:
    result = number(value)
    return None if result is None else int(round(result))


@dataclass(frozen=True)
class Field:
    """A field written to the target file, read from an export field and converted."""

    name: str
    kind: str
    size: int
    decimals: int
    source: str
    convert: Callable | None = None

    @property
    def spec(self) -> list:
        return [self.name, self.kind, self.size, self.decimals]


@dataclass(frozen=True)
class Shape:
    """Where a shape comes from and how it is written."""

    dataset: str
    target: str
    # None: the export's own fields, as they are
    fields: tuple[Field, ...] | None = None
    # The field that identifies a feature, for the summary and for keeping dropped features
    key: str | None = None
    keep_dropped: bool = False
    # The export's real projection when it isn't WGS 84 (its .prj can't be trusted)
    source_crs: str | None = None

    @property
    def path(self) -> Path:
        return FILES / self.target


def _text(name: str, source: str | None = None) -> Field:
    return Field(name, "C", 254, 0, source or name)


LANDMARK_DATES = (
    Field("caldate", "C", 254, 0, "date_calda", us_date),
    Field("desdate", "C", 254, 0, "date_desda", us_date),
    Field("current_", "C", 254, 0, "current_", yes_no),
    Field("shape_area", "N", 33, 31, "shape_area", number),
    Field("shape_leng", "N", 33, 31, "shape_leng", number),
)

SHAPES: dict[str, Shape] = {
    # The borough shape moves from the NAD83 release to the WGS 84 export, with the field
    # names (and types) of the NAD83 file the mapping profile reads
    "BoroughBoundaries": Shape(
        dataset="gthc-hcne",
        target="Borough_Boundaries/WGS84/nybb",
        fields=(
            Field("BoroCode", "N", 10, 0, "borocode", whole),
            Field("BoroName", "C", 32, 0, "boroname"),
            Field("Shape_Leng", "F", 19, 11, "shape_leng", number),
            Field("Shape_Area", "F", 19, 11, "shape_area", number),
        ),
        key="BoroCode",
    ),
    "HistoricDistricts": Shape(
        dataset="skyk-mpzq",
        target="Historic_Districts/WGS84/Historic_Districts",
        fields=(
            _text("area_name"), _text("borough"), _text("boundary_n"), _text("extension"),
            _text("last_actio"), _text("lp_number"), _text("other_hear"), _text("public_hea"),
            _text("status_of_"),
            *LANDMARK_DATES,
        ),
        key="lp_number",
        keep_dropped=True,
        source_crs="EPSG:2263",
    ),
    "ScenicLandmarks": Shape(
        dataset="qexa-qpj6",
        target="Scenic_Landmarks/WGS84/Scenic_Landmarks",
        fields=(
            _text("borough"), _text("last_actio"), _text("lp_number"), _text("public_hea"),
            _text("scen_lm_na"), _text("status"),
            *LANDMARK_DATES,
        ),
        key="lp_number",
        keep_dropped=True,
        source_crs="EPSG:2263",
    ),
    "PrivatelyOwnedPublicSpaces": Shape(
        dataset="rvih-nhyn", target="Privately_Owned_Public_Spaces/WGS84/POPS", key="pops_numbe"),
    "Greenstreets": Shape(dataset="mk9u-qu7i", target="Greenstreets/WGS84/Greenstreets", key="gispropnum"),
    "Libraries": Shape(dataset="feuq-due4", target="Libraries/WGS84/Libraries", key="name"),
    "PostOffices": Shape(dataset="bdha-6eqy", target="Post_Offices/WGS84/Post_Offices", key="name"),
}


@dataclass
class Layer:
    """A shapefile's fields, shapes and records, in file order."""

    fields: list
    shapes: list
    records: list
    shape_type: int
    prj: str = ""
    names: list[str] = field(init=False)

    def __post_init__(self) -> None:
        self.names = [f[0] for f in self.fields]

    def keys(self, key: str | None) -> list[str]:
        if not key or key not in self.names:
            return []
        index = self.names.index(key)
        return [str(r[index]).strip() for r in self.records]


def read_layer(path: Path) -> Layer:
    reader = shapefile.Reader(str(path), encoding="utf-8", encodingErrors="replace")
    prj = path.with_suffix(".prj")
    layer = Layer(
        fields=[list(f) for f in reader.fields[1:]],
        shapes=reader.shapes(),
        records=[list(r) for r in reader.records()],
        shape_type=reader.shapeType,
        prj=prj.read_text(encoding="utf-8") if prj.exists() else "",
    )
    reader.close()
    return layer


def download(dataset: str) -> bytes:
    connection = http.client.HTTPSConnection(DATASET_HOST, timeout=120)
    try:
        connection.request("GET", EXPORT_PATH.format(id=dataset), headers={"User-Agent": USER_AGENT})
        response = connection.getresponse()
        body = response.read()
        if response.status != 200:
            raise RuntimeError(f"{dataset}: export returned {response.status}")
        return body
    finally:
        connection.close()


def read_export(archive: bytes) -> Layer:
    """The one shapefile in an export zip."""
    with tempfile.TemporaryDirectory() as folder, zipfile.ZipFile(io.BytesIO(archive)) as zipped:
        zipped.extractall(folder)
        shp = next(Path(folder).glob("*.shp"))
        return read_layer(shp.with_suffix(""))


def reproject(layer: Layer, source_crs: str | None) -> Layer:
    """The layer's shapes in WGS 84 lon/lat, from `source_crs` when it is given."""
    if not source_crs:
        return layer
    transform = Transformer.from_crs(source_crs, "EPSG:4326", always_xy=True).transform
    shapes = []
    for shp in layer.shapes:
        moved = shapefile.Shape(shapeType=shp.shapeType)
        moved.points = [transform(x, y) for x, y in shp.points]
        moved.parts = list(shp.parts)
        shapes.append(moved)
    return Layer(layer.fields, shapes, layer.records, layer.shape_type, layer.prj)


def convert(shape: Shape, export: Layer) -> Layer:
    """The export with the shape's fields, or as it is when the shape takes the export's own."""
    export = reproject(export, shape.source_crs)
    if shape.fields is None:
        return export
    missing = [f.source for f in shape.fields if f.source not in export.names]
    if missing:
        raise RuntimeError(f"{shape.dataset}: the export has no field {', '.join(missing)}")
    indexes = [export.names.index(f.source) for f in shape.fields]
    records = [
        [(f.convert(r[i]) if f.convert else r[i]) for f, i in zip(shape.fields, indexes)]
        for r in export.records
    ]
    return Layer([f.spec for f in shape.fields], export.shapes, records, export.shape_type, export.prj)


def merge_kept(shape: Shape, fresh: Layer, current: Layer | None) -> tuple[Layer, list[str]]:
    """The fresh file plus the current file's features whose key it no longer has, marked kept."""
    if not shape.keep_dropped or current is None or not shape.key:
        return fresh, []
    fresh_keys = set(fresh.keys(shape.key))
    current_index = {name: i for i, name in enumerate(current.names)}
    kept_rows = [
        (s, r) for s, r, k in zip(current.shapes, current.records, current.keys(shape.key))
        if k not in fresh_keys
    ]
    fields = fresh.fields + [KEPT_FIELD]
    records = [r + [""] for r in fresh.records]
    for _, record in kept_rows:
        records.append([record[current_index[f[0]]] if f[0] in current_index else "" for f in fresh.fields] + ["Yes"])
    shapes = list(fresh.shapes) + [s for s, _ in kept_rows]
    kept = [str(r[current_index[shape.key]]).strip() for _, r in kept_rows]
    return Layer(fields, shapes, records, fresh.shape_type, fresh.prj), kept


def compact(fields: list, records: list) -> list:
    """Text fields as wide as their longest value (UTF-8 bytes), not the export's 254 each."""
    specs = []
    for i, (name, kind, size, decimals) in enumerate(fields):
        if kind == "C":
            size = max([1, *(len(str(r[i] if r[i] is not None else "").encode("utf-8")) for r in records)])
        specs.append([name, kind, min(size, 254), decimals])
    return specs


def write_layer(path: Path, layer: Layer, prj: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with shapefile.Writer(str(path), shapeType=layer.shape_type, encoding="utf-8") as writer:
        for spec in compact(layer.fields, layer.records):
            writer.field(*spec)
        for shp, record in zip(layer.shapes, layer.records):
            writer.shape(shp)
            writer.record(*record)
    path.with_suffix(".cpg").write_text("UTF-8", encoding="ascii")
    if prj:
        path.with_suffix(".prj").write_text(prj, encoding="utf-8")


def summary(name: str, shape: Shape, current: Layer | None, result: Layer, kept: list[str]) -> str:
    before = set(current.keys(shape.key)) if current and shape.key in current.names else set()
    after = set(result.keys(shape.key))
    lines = [f"{name}: {len(current.records) if current else 0} -> {len(result.records)} rows ({shape.dataset})"]
    if current and shape.key and shape.key in current.names:
        added = sorted(after - before)
        dropped = sorted(before - after)
        lines.append(f"  added {len(added)}: {', '.join(added[:20])}{' ...' if len(added) > 20 else ''}")
        lines.append(f"  dropped {len(dropped)}: {', '.join(dropped[:20])}")
    if kept:
        lines.append(f"  kept {len(kept)}: {', '.join(kept)}")
    return "\n".join(lines)


def refresh(name: str, check: bool, fetch: Callable[[str], bytes] = download) -> str:
    shape = SHAPES[name]
    export = read_export(fetch(shape.dataset))
    current = read_layer(shape.path) if shape.path.with_suffix(".shp").exists() else None
    result, kept = merge_kept(shape, convert(shape, export), current)
    if not check:
        # A refreshed file keeps its own .prj; a new one takes the export's
        write_layer(shape.path, result, "" if current and current.prj else export.prj)
    return summary(name, shape, current, result, kept)


def parse_args(argv: list[str] | None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Refresh shapes in files/ from NYC Open Data.")
    parser.add_argument("shapes", nargs="*", metavar="SHAPE",
                        help=f"shapes to refresh (default: all): {', '.join(SHAPES)}")
    parser.add_argument("--check", action="store_true", help="compare only; write nothing")
    args = parser.parse_args(argv)
    unknown = [name for name in args.shapes if name not in SHAPES]
    if unknown:
        parser.error(f"unknown shape {', '.join(unknown)}; choose from {', '.join(SHAPES)}")
    return args


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    for name in args.shapes or list(SHAPES):
        print(refresh(name, args.check))
    return 0


if __name__ == "__main__":
    sys.exit(main())
