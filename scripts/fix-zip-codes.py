#!/usr/bin/env python3
"""Replace retired ZIP codes in files/Zip_Codes (ShapeProperties.ZipCodes).

The ZIP boundaries are the city's 2014 file (ZIP_CODE_040114); NYC Open Data now
has only the Census ZIP Code Tabulation Areas, a different geography, so the file
isn't refreshed. A code the Postal Service has since retired is replaced here, in
the .dbf only: the outline stays, so a point inside it gets the code that now
serves it. Each replacement says why.

    python3 scripts/fix-zip-codes.py          # replace, and print what changed
    python3 scripts/fix-zip-codes.py --check  # exit 1 if a retired code is left

Standard library only: the ZIPCODE field is fixed-width text, rewritten in place,
so the record count, field layout and geometry are unchanged.
"""

from __future__ import annotations

import argparse
import struct
import sys
from pathlib import Path

DBF = Path(__file__).resolve().parent.parent / "files" / "Zip_Codes" / "ZIP_CODE_040114.dbf"
FIELD = "ZIPCODE"

# Retired code -> the code that serves the same ground now, and why
RETIRED: dict[str, tuple[str, str]] = {
    "10048": (
        "10007",
        "The World Trade Center's own code, retired after 2001; the rebuilt site "
        "(One World Trade Center, 285 Fulton Street) is New York, NY 10007.",
    ),
}


def _layout(data: bytes, field: str) -> tuple[int, int, int, int, int]:
    """(records, header length, record length, field offset in a record, field length)."""
    records, header, record = struct.unpack("<IHH", data[4:12])
    offset = 1  # the deletion flag
    position = 32
    while data[position] != 0x0D:
        name = data[position : position + 11].split(b"\0")[0].decode("ascii")
        length = data[position + 16]
        if name == field:
            return records, header, record, offset, length
        offset += length
        position += 32
    raise ValueError(f"{field} is not a field of the file")


def fix(path: Path, retired: dict[str, tuple[str, str]], write: bool = True) -> list[tuple[int, str, str]]:
    """Replace each retired code; (record number, old, new) for each record changed (or to change)."""
    data = bytearray(path.read_bytes())
    records, header, record, offset, length = _layout(bytes(data), FIELD)
    changed: list[tuple[int, str, str]] = []
    for number in range(records):
        start = header + number * record + offset
        value = data[start : start + length].decode("latin-1").strip()
        if value in retired:
            new = retired[value][0]
            data[start : start + length] = new.ljust(length).encode("latin-1")
            changed.append((number, value, new))
    if changed and write:
        path.write_bytes(bytes(data))
    return changed


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Replace retired ZIP codes in files/Zip_Codes.")
    parser.add_argument("--check", action="store_true", help="exit 1 if a retired code is left; change nothing")
    parser.add_argument("--dbf", type=Path, default=DBF, help=argparse.SUPPRESS)
    args = parser.parse_args(argv)

    changed = fix(args.dbf, RETIRED, write=not args.check)
    for number, old, new in changed:
        verb = "would replace" if args.check else "replaced"
        print(f"record {number}: {verb} {old} with {new} ({RETIRED[old][1]})")
    if args.check:
        return 1 if changed else 0
    if not changed:
        print("No retired ZIP codes left.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
