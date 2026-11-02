#!/usr/bin/env python3
"""Add historic district outlines from LPC's Open Data to files/Historic_Districts/WGS84.

The service reads the WGS84 shapefile (ShapeProperties.HistoricDistricts). A
district LPC designated after the file was last refreshed has no outline, so
CoreDataStore.UI can't draw its map. This script fetches the named districts
from NYC Open Data "Historic Districts" (skyk-mpzq), reprojects them from NY
State Plane feet (EPSG:2263) to WGS84, and appends them with the same fields.
Districts already in the file are left as they are, except one stored with an
unpadded LP number (LP-2665 for LP-02665): the UI and the attribute lookup use
the padded form, so that record is replaced by Open Data's.

    pip install pyshp==2.3.1 pyproj==3.6.1
    python3 scripts/add-historic-districts.py LP-02665 LP-02683

Rebuilding an existing district from Open Data this way matches its outline in
the file to the centimetre (checked on LP-02647, 2026-11-02).
"""

from __future__ import annotations

import argparse
import http.client
import json
import re
import sys
import urllib.parse
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path

import shapefile
from pyproj import Transformer

# Fetched over HTTPS from this host only
DATASET_HOST = "data.cityofnewyork.us"
DATASET_PATH = "/resource/skyk-mpzq.geojson"
SHAPEFILE = Path(__file__).resolve().parent.parent / "files" / "Historic_Districts" / "WGS84" / "Historic_Districts"
USER_AGENT = "MicroService-add-historic-districts (+https://github.com/stuartshay/MicroService)"
DATE_FIELDS = ("caldate", "desdate", "public_hea")
LP_PATTERN = re.compile(r"^LP-\d{5}$")


@dataclass
class Districts:
    """The shapefile's fields, shapes and records, in file order."""

    fields: list
    shapes: list
    records: list
    names: list[str] = field(init=False)

    def __post_init__(self) -> None:
        self.names = [f[0] for f in self.fields]

    @property
    def lp_index(self) -> int:
        return self.names.index("lp_number")


def padded(lp_number: str) -> str:
    """LP-2665 -> LP-02665; anything else is returned as it is."""
    match = re.fullmatch(r"LP-0*(\d{1,5})", lp_number.strip().upper())
    return f"LP-{int(match.group(1)):05d}" if match else lp_number


def fetch(lp_numbers: list[str]) -> list[dict]:
    where = "lp_number in (" + ",".join(f"'{lp}'" for lp in lp_numbers) + ")"
    connection = http.client.HTTPSConnection(DATASET_HOST, timeout=60)
    try:
        connection.request("GET", f"{DATASET_PATH}?{urllib.parse.urlencode({'$where': where})}",
                           headers={"User-Agent": USER_AGENT})
        response = connection.getresponse()
        if response.status != 200:
            raise RuntimeError(f"Open Data returned {response.status} {response.reason}")
        return json.load(response)["features"]
    finally:
        connection.close()


def shapefile_date(value: str | None) -> str:
    """Open Data's 2023-05-09T00:00:00.000 as the file's 5/9/2023; blank stays blank."""
    if not value:
        return ""
    if re.fullmatch(r"\d{1,2}/\d{1,2}/\d{4}", value):
        return value
    parsed = datetime.fromisoformat(value[:10])
    return f"{parsed.month}/{parsed.day}/{parsed.year}"


def ascii_text(value) -> str:
    """The file's text is ASCII; LPC's names can carry an en dash."""
    text = "" if value is None else str(value)
    return text.replace("\u2013", "-").replace("\u2014", "-").encode("ascii", "replace").decode("ascii")


def signed_area(ring: list[tuple[float, float]]) -> float:
    return sum(x1 * y2 - x2 * y1 for (x1, y1), (x2, y2) in zip(ring, ring[1:] + ring[:1])) / 2


def shapefile_parts(geometry: dict, transform) -> list[list[tuple[float, float]]]:
    """Rings in WGS84, outer rings clockwise and holes counter-clockwise, as the shapefile format wants."""
    polygons = [geometry["coordinates"]] if geometry["type"] == "Polygon" else geometry["coordinates"]
    parts = []
    for polygon in polygons:
        for index, ring in enumerate(polygon):
            points = [transform(x, y) for x, y in ring]
            clockwise = signed_area(points) < 0
            if (index == 0) != clockwise:
                points.reverse()
            parts.append(points)
    return parts


def record_values(fields: list, props: dict) -> list:
    """Open Data's properties in the file's field order and formats."""
    values = []
    for name, kind, _, _ in fields:
        value = props.get(name)
        if kind == "N":
            values.append(float(value) if value not in (None, "") else None)
        elif name in DATE_FIELDS:
            values.append(shapefile_date(value))
        else:
            values.append(ascii_text(value))
    return values


def read_districts() -> Districts:
    reader = shapefile.Reader(str(SHAPEFILE))
    try:
        return Districts(reader.fields[1:], reader.shapes(), reader.records())
    finally:
        reader.close()


def stale_records(districts: Districts, wanted: list[str]) -> dict[int, str]:
    """Records for a wanted district stored under another spelling of its LP number (LP-2665)."""
    stale = {}
    for i, record in enumerate(districts.records):
        lp = record[districts.lp_index]
        if lp != padded(lp) and padded(lp) in wanted:
            stale[i] = padded(lp)
    return stale


def build_additions(districts: Districts, lp_numbers: list[str], dry_run: bool) -> tuple[list, list[str]]:
    """(parts, values) for each district found in Open Data, and the LP numbers it doesn't have."""
    features = {f["properties"]["lp_number"]: f for f in fetch(lp_numbers)}
    transform = Transformer.from_crs("EPSG:2263", "EPSG:4326", always_xy=True).transform
    additions, not_found = [], []
    for lp in lp_numbers:
        feature = features.get(lp)
        if feature is None:
            not_found.append(lp)
            print(f"error  {lp}: not in Open Data skyk-mpzq", file=sys.stderr)
            continue
        parts = shapefile_parts(feature["geometry"], transform)
        additions.append((parts, record_values(districts.fields, feature["properties"])))
        print(f"{'would add' if dry_run else 'add'}    {lp} {ascii_text(feature['properties'].get('area_name'))} "
              f"({sum(len(p) for p in parts)} points)")
    return additions, not_found


def write_districts(districts: Districts, additions: list, skip: set[int]) -> int:
    """Rewrite the file: every record not in `skip`, then the additions. Returns the new count."""
    writer = shapefile.Writer(str(SHAPEFILE), shapeType=shapefile.POLYGON)
    for f in districts.fields:
        writer.field(*f)
    count = 0
    for i, (shape, record) in enumerate(zip(districts.shapes, districts.records)):
        if i not in skip:
            writer.shape(shape)
            writer.record(*record)
            count += 1
    for parts, values in additions:
        writer.poly(parts)
        writer.record(*values)
        count += 1
    writer.close()
    return count


def parse_args(argv: list[str] | None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Add historic district outlines from LPC Open Data to the WGS84 shapefile.")
    parser.add_argument("lp_numbers", nargs="+", help="district LP numbers, e.g. LP-02665")
    parser.add_argument("--dry-run", action="store_true", help="report what would be added, write nothing")
    args = parser.parse_args(argv)
    args.lp_numbers = sorted({lp.strip().upper() for lp in args.lp_numbers})
    bad = [lp for lp in args.lp_numbers if not LP_PATTERN.match(lp)]
    if bad:
        parser.error(f"not LP numbers: {', '.join(bad)}")
    return args


def report_present(districts: Districts, lp_numbers: list[str], stale: dict[int, str], verb: str) -> list[str]:
    """Print what the file already has and what it will replace; return the LP numbers to fetch."""
    present = {record[districts.lp_index] for record in districts.records}
    for lp in lp_numbers:
        if lp in present:
            print(f"have   {lp}")
    for i, lp in stale.items():
        print(f"{verb} record {i} {districts.records[i][districts.lp_index]!r} with {lp}")
    return [lp for lp in lp_numbers if lp not in present]


def apply(districts: Districts, additions: list, stale: dict[int, str]) -> None:
    added = {values[districts.lp_index] for _, values in additions}
    skip = {i for i, lp in stale.items() if lp in added}
    count = write_districts(districts, additions, skip)
    print(f"wrote  {SHAPEFILE.name}: {len(districts.records)} - {len(skip)} replaced + {len(additions)} = {count} districts")


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    districts = read_districts()
    stale = stale_records(districts, args.lp_numbers)
    missing = report_present(districts, args.lp_numbers, stale, "would replace" if args.dry_run else "replace")
    if not missing:
        return 0
    additions, not_found = build_additions(districts, missing, args.dry_run)
    if additions and not args.dry_run:
        apply(districts, additions, stale)
    return 1 if not_found else 0


if __name__ == "__main__":
    sys.exit(main())
