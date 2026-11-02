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
import json
import re
import sys
import urllib.parse
import urllib.request
from datetime import datetime
from pathlib import Path

import shapefile
from pyproj import Transformer

DATASET = "https://data.cityofnewyork.us/resource/skyk-mpzq.geojson"
SHAPEFILE = Path(__file__).resolve().parent.parent / "files" / "Historic_Districts" / "WGS84" / "Historic_Districts"
USER_AGENT = "MicroService-add-historic-districts (+https://github.com/stuartshay/MicroService)"
DATE_FIELDS = ("caldate", "desdate", "public_hea")
LP_PATTERN = re.compile(r"^LP-\d{5}$")


def padded(lp_number: str) -> str:
    """LP-2665 -> LP-02665; anything else is returned as it is."""
    match = re.fullmatch(r"LP-0*(\d{1,5})", lp_number.strip().upper())
    return f"LP-{int(match.group(1)):05d}" if match else lp_number


def fetch(lp_numbers: list[str]) -> list[dict]:
    where = "lp_number in (" + ",".join(f"'{lp}'" for lp in lp_numbers) + ")"
    url = f"{DATASET}?{urllib.parse.urlencode({'$where': where})}"
    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
    with urllib.request.urlopen(request, timeout=60) as response:
        return json.load(response)["features"]


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
    return text.replace("–", "-").replace("—", "-").encode("ascii", "replace").decode("ascii")


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


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Add historic district outlines from LPC Open Data to the WGS84 shapefile.")
    parser.add_argument("lp_numbers", nargs="+", help="district LP numbers, e.g. LP-02665")
    parser.add_argument("--dry-run", action="store_true", help="report what would be added, write nothing")
    args = parser.parse_args(argv)

    wanted = sorted({lp.strip().upper() for lp in args.lp_numbers})
    bad = [lp for lp in wanted if not LP_PATTERN.match(lp)]
    if bad:
        parser.error(f"not LP numbers: {', '.join(bad)}")

    reader = shapefile.Reader(str(SHAPEFILE))
    fields = reader.fields[1:]
    names = [f[0] for f in fields]
    shapes, records = reader.shapes(), reader.records()
    reader.close()
    lp_index = names.index("lp_number")
    present = {record[lp_index] for record in records}
    # Records for a wanted district stored under another spelling of its LP number (LP-2665)
    stale = {i: padded(r[lp_index]) for i, r in enumerate(records)
             if padded(r[lp_index]) in wanted and r[lp_index] != padded(r[lp_index])}

    missing = [lp for lp in wanted if lp not in present]
    for lp in wanted:
        if lp in present:
            print(f"have   {lp}")
    for i, lp in stale.items():
        print(f"{'would replace' if args.dry_run else 'replace'} record {i} {records[i][lp_index]!r} with {lp}")
    if not missing:
        return 0

    features = {f["properties"]["lp_number"]: f for f in fetch(missing)}
    not_found = [lp for lp in missing if lp not in features]
    for lp in not_found:
        print(f"error  {lp}: not in Open Data skyk-mpzq", file=sys.stderr)

    transform = Transformer.from_crs("EPSG:2263", "EPSG:4326", always_xy=True).transform
    additions = []
    for lp in missing:
        if lp not in features:
            continue
        feature = features[lp]
        props = feature["properties"]
        values = []
        for name, kind, _, _ in fields:
            value = props.get(name)
            if kind == "N":
                values.append(float(value) if value not in (None, "") else None)
            elif name in DATE_FIELDS:
                values.append(shapefile_date(value))
            else:
                values.append(ascii_text(value))
        parts = shapefile_parts(feature["geometry"], transform)
        additions.append((parts, values))
        print(f"{'would add' if args.dry_run else 'add'}    {lp} {ascii_text(props.get('area_name'))} "
              f"({sum(len(p) for p in parts)} points)")

    if args.dry_run or not additions:
        return 1 if not_found else 0

    writer = shapefile.Writer(str(SHAPEFILE), shapeType=shapefile.POLYGON)
    for field in fields:
        writer.field(*field)
    replaced = {lp for lp in stale.values() if lp in {v[lp_index] for _, v in additions}}
    for i, (shape, record) in enumerate(zip(shapes, records)):
        if stale.get(i) in replaced:
            continue
        writer.shape(shape)
        writer.record(*record)
    for parts, values in additions:
        writer.poly(parts)
        writer.record(*values)
    writer.close()
    print(f"wrote  {SHAPEFILE.name}: {len(records)} - {len(replaced)} replaced + {len(additions)} = "
          f"{len(records) - len(replaced) + len(additions)} districts")
    return 1 if not_found else 0


if __name__ == "__main__":
    sys.exit(main())
