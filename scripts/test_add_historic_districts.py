"""Checks for add-historic-districts.py without network access or its shapefile/pyproj dependencies."""

import importlib.util
from pathlib import Path
import sys
import types
import unittest
from unittest import mock

SCRIPT = Path(__file__).with_name("add-historic-districts.py")


def load_script():
    """Import the script with stand-ins for pyshp and pyproj, which CI doesn't install."""
    fakes = {"shapefile": types.ModuleType("shapefile"), "pyproj": types.ModuleType("pyproj")}
    fakes["pyproj"].Transformer = object
    spec = importlib.util.spec_from_file_location("add_historic_districts", SCRIPT)
    module = importlib.util.module_from_spec(spec)
    with mock.patch.dict(sys.modules, {**fakes, spec.name: module}):
        spec.loader.exec_module(module)
    return module


script = load_script()
FIELDS = [["area_name", "C", 254, 0], ["borough", "C", 254, 0], ["desdate", "C", 254, 0],
          ["lp_number", "C", 254, 0], ["shape_area", "N", 33, 31]]


class HelpersTest(unittest.TestCase):
    def test_padded(self):
        self.assertEqual(script.padded("LP-2665"), "LP-02665")
        self.assertEqual(script.padded("LP-00099"), "LP-00099")
        self.assertEqual(script.padded("lp-099"), "LP-00099")
        self.assertEqual(script.padded("not an LP"), "not an LP")

    def test_dates_use_the_files_format(self):
        self.assertEqual(script.shapefile_date("2023-05-09T00:00:00.000"), "5/9/2023")
        self.assertEqual(script.shapefile_date("9/22/2020"), "9/22/2020")
        self.assertEqual(script.shapefile_date(None), "")
        self.assertEqual(script.shapefile_date(""), "")

    def test_text_is_ascii(self):
        self.assertEqual(script.ascii_text("Central Harlem – West 130th"), "Central Harlem - West 130th")
        self.assertEqual(script.ascii_text(None), "")
        self.assertEqual(script.ascii_text("Brooklyn\u00a0Heights"), "Brooklyn?Heights")

    def test_record_values_follow_the_field_order_and_types(self):
        props = {"lp_number": "LP-02683", "area_name": "Willoughby-Hart Historic District", "borough": "BK",
                 "desdate": "2024-06-25T00:00:00.000", "shape_area": "372947.68809495633"}
        self.assertEqual(script.record_values(FIELDS, props),
                         ["Willoughby-Hart Historic District", "BK", "6/25/2024", "LP-02683", 372947.68809495633])
        self.assertIsNone(script.record_values(FIELDS, {})[4])


class RingsTest(unittest.TestCase):
    square = [[0, 0], [1, 0], [1, 1], [0, 1], [0, 0]]  # counter-clockwise

    def test_outer_rings_become_clockwise_and_holes_counter_clockwise(self):
        hole = [[0.2, 0.2], [0.2, 0.8], [0.8, 0.8], [0.8, 0.2], [0.2, 0.2]]  # clockwise
        parts = script.shapefile_parts({"type": "Polygon", "coordinates": [self.square, hole]}, lambda x, y: (x, y))
        self.assertLess(script.signed_area(parts[0]), 0)
        self.assertGreater(script.signed_area(parts[1]), 0)

    def test_multipolygons_give_one_part_per_ring_and_are_transformed(self):
        geometry = {"type": "MultiPolygon", "coordinates": [[self.square], [self.square]]}
        parts = script.shapefile_parts(geometry, lambda x, y: (x * 10, y * 10))
        self.assertEqual(len(parts), 2)
        self.assertIn((10, 10), parts[0])


class StaleRecordsTest(unittest.TestCase):
    def test_unpadded_records_of_wanted_districts_are_stale(self):
        records = [["A", "MN", "", "LP-2607", 1.0], ["B", "BK", "", "LP-00099", 2.0], ["C", "BK", "", "LP-2665", 3.0]]
        districts = script.Districts(FIELDS, [None] * 3, records)
        self.assertEqual(script.stale_records(districts, ["LP-02607", "LP-02683"]), {0: "LP-02607"})


class ArgumentsTest(unittest.TestCase):
    def test_lp_numbers_are_normalized_and_checked(self):
        self.assertEqual(script.parse_args(["lp-02683", "LP-02665", "LP-02683"]).lp_numbers, ["LP-02665", "LP-02683"])
        with mock.patch("sys.stderr"), self.assertRaises(SystemExit):
            script.parse_args(["LP-2665"])


class FetchTest(unittest.TestCase):
    def test_fetch_uses_https_to_the_dataset_host(self):
        response = mock.Mock(status=200)
        response.read.return_value = b'{"features": [{"properties": {"lp_number": "LP-02683"}}]}'
        with mock.patch.object(script.http.client, "HTTPSConnection") as https:
            https.return_value.getresponse.return_value = response
            with mock.patch.object(script.json, "load", lambda _response: {"features": [1]}):
                self.assertEqual(script.fetch(["LP-02683"]), [1])
        https.assert_called_once_with("data.cityofnewyork.us", timeout=60)
        path = https.return_value.request.call_args.args[1]
        self.assertTrue(path.startswith("/resource/skyk-mpzq.geojson?%24where=lp_number+in+%28%27LP-02683%27%29"))

    def test_fetch_raises_on_an_error_status(self):
        with mock.patch.object(script.http.client, "HTTPSConnection") as https:
            https.return_value.getresponse.return_value = mock.Mock(status=503, reason="Service Unavailable")
            with self.assertRaisesRegex(RuntimeError, "503"):
                script.fetch(["LP-02683"])


if __name__ == "__main__":
    unittest.main()
