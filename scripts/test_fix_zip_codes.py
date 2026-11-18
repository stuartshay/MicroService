#!/usr/bin/env python3
"""Tests for fix-zip-codes.py, on a small .dbf built here."""

import contextlib
import importlib.util
import io
import struct
import tempfile
import unittest
from pathlib import Path

SCRIPT = Path(__file__).resolve().parent / "fix-zip-codes.py"


def load_script():
    spec = importlib.util.spec_from_file_location("fix_zip_codes", SCRIPT)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


fix_zip_codes = load_script()


def write_dbf(path: Path, rows: list[tuple[str, str]]) -> None:
    """A dBASE III file with ZIPCODE (C 5) and PO_NAME (C 10)."""
    fields = [("ZIPCODE", 5), ("PO_NAME", 10)]
    record = 1 + sum(length for _, length in fields)
    header = 32 + 32 * len(fields) + 1
    data = bytearray(struct.pack("<4B I 2H 20x", 3, 126, 11, 18, len(rows), header, record))
    for name, length in fields:
        data += name.encode("ascii").ljust(11, b"\0") + b"C" + b"\0" * 4 + bytes([length, 0]) + b"\0" * 14
    data += b"\r"
    for zip_code, po_name in rows:
        data += b" " + zip_code.ljust(5).encode("ascii") + po_name.ljust(10).encode("ascii")
    data += b"\x1a"
    path.write_bytes(bytes(data))


def values(path: Path) -> list[tuple[str, str]]:
    data = path.read_bytes()
    records, header, record = struct.unpack("<IHH", data[4:12])
    rows = []
    for number in range(records):
        start = header + number * record
        rows.append((data[start + 1 : start + 6].decode().strip(), data[start + 6 : start + 16].decode().strip()))
    return rows


class FixZipCodesTest(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.dbf = Path(self.directory.name) / "zips.dbf"
        write_dbf(self.dbf, [("10007", "New York"), ("10048", "New York"), ("00083", "Central Pk")])

    def tearDown(self):
        self.directory.cleanup()

    def run_main(self, *args):
        output = io.StringIO()
        with contextlib.redirect_stdout(output):
            code = fix_zip_codes.main([*args, "--dbf", str(self.dbf)])
        return code, output.getvalue()

    def test_replaces_a_retired_code_and_leaves_every_other_value(self):
        before = self.dbf.read_bytes()
        code, output = self.run_main()

        self.assertEqual(0, code)
        self.assertEqual([("10007", "New York"), ("10007", "New York"), ("00083", "Central Pk")], values(self.dbf))
        self.assertIn("record 1: replaced 10048 with 10007", output)
        # Only bytes inside that record's ZIPCODE field changed
        after = self.dbf.read_bytes()
        self.assertEqual(len(before), len(after))
        header, record = struct.unpack("<HH", before[8:12])
        field_start = header + 1 * record + 1
        changed = [i for i, (a, b) in enumerate(zip(before, after)) if a != b]
        self.assertTrue(changed)
        self.assertTrue(all(field_start <= i < field_start + 5 for i in changed), changed)

    def test_running_again_changes_nothing(self):
        self.run_main()
        before = self.dbf.read_bytes()
        code, output = self.run_main()

        self.assertEqual(0, code)
        self.assertEqual(before, self.dbf.read_bytes())
        self.assertIn("No retired ZIP codes left.", output)

    def test_check_reports_without_writing(self):
        before = self.dbf.read_bytes()
        code, output = self.run_main("--check")

        self.assertEqual(1, code)
        self.assertEqual(before, self.dbf.read_bytes())
        self.assertIn("would replace 10048 with 10007", output)

        self.run_main()
        self.assertEqual(0, self.run_main("--check")[0])

    def test_the_committed_file_has_no_retired_codes(self):
        self.assertEqual([], fix_zip_codes.fix(fix_zip_codes.DBF, fix_zip_codes.RETIRED, write=False))


if __name__ == "__main__":
    unittest.main()
