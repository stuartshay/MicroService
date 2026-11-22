"""Unit tests for refresh-shapes.py (no network). Needs pyshp and pyproj."""

import importlib.util
import sys
import unittest
from datetime import date
from pathlib import Path

SCRIPT = Path(__file__).resolve().parent / "refresh-shapes.py"


def load():
    spec = importlib.util.spec_from_file_location("refresh_shapes", SCRIPT)
    module = importlib.util.module_from_spec(spec)
    sys.modules["refresh_shapes"] = module
    spec.loader.exec_module(module)
    return module


rs = load()


def layer(fields, rows, shape_type=1):
    """A layer of points at (i, i) with the given fields and records."""
    import shapefile

    shapes = []
    for i, _ in enumerate(rows):
        point = shapefile.Shape(shapeType=shape_type)
        point.points = [(float(i), float(i))]
        point.parts = [0]
        shapes.append(point)
    return rs.Layer([list(f) for f in fields], shapes, [list(r) for r in rows], shape_type)


class ConversionTest(unittest.TestCase):
    def test_dates_and_yes_no(self):
        self.assertEqual(rs.us_date(date(2025, 11, 25)), "11/25/2025")
        self.assertEqual(rs.us_date(None), "")
        self.assertEqual(rs.yes_no("true"), "Yes")
        self.assertEqual(rs.yes_no("FALSE"), "No")
        self.assertEqual(rs.yes_no("Yes"), "Yes")
        self.assertEqual(rs.whole("1.0000"), 1)
        self.assertIsNone(rs.number("n/a"))

    def test_convert_renames_and_converts_fields(self):
        shape = rs.Shape(dataset="x", target="t", fields=(
            rs.Field("lp_number", "C", 254, 0, "lp_number"),
            rs.Field("desdate", "C", 254, 0, "date_desda", rs.us_date),
        ))
        export = layer([["lp_number", "C", 254, 0], ["date_desda", "D", 8, 0]], [["LP-00099", date(1965, 11, 23)]])
        result = rs.convert(shape, export)
        self.assertEqual([f[0] for f in result.fields], ["lp_number", "desdate"])
        self.assertEqual(result.records, [["LP-00099", "11/23/1965"]])

    def test_convert_refuses_an_export_without_a_field(self):
        shape = rs.Shape(dataset="x", target="t", fields=(rs.Field("a", "C", 1, 0, "missing"),))
        with self.assertRaises(RuntimeError):
            rs.convert(shape, layer([["other", "C", 1, 0]], [["v"]]))

    def test_reproject_state_plane_feet_to_lon_lat(self):
        # The pair AbstractShapeServiceTest converts: NY State Plane Long Island feet to WGS 84
        feet = layer([["k", "C", 1, 0]], [["a"]])
        feet.shapes[0].points = [(1021192.94, 212550.02)]
        x, y = rs.reproject(feet, "EPSG:2263").shapes[0].points[0]
        self.assertAlmostEqual(x, -73.866667, places=5)
        self.assertAlmostEqual(y, 40.750000, places=5)
        # Without a source projection the layer is left as it is
        self.assertIs(rs.reproject(feet, None), feet)


class KeepTest(unittest.TestCase):
    shape = rs.Shape(dataset="x", target="t", key="lp_number", keep_dropped=True)
    fields = [["lp_number", "C", 254, 0], ["name", "C", 254, 0]]

    def test_keeps_the_dropped_features_marked(self):
        fresh = layer(self.fields, [["LP-1", "One (city)"]])
        current = layer(self.fields, [["LP-1", "One"], ["LP-2", "Two"]])
        merged, kept = rs.merge_kept(self.shape, fresh, current)
        self.assertEqual(kept, ["LP-2"])
        self.assertEqual([f[0] for f in merged.fields], ["lp_number", "name", "kept"])
        self.assertEqual(merged.records, [["LP-1", "One (city)", ""], ["LP-2", "Two", "Yes"]])
        self.assertEqual(len(merged.shapes), 2)

    def test_takes_the_fresh_file_alone_without_keep_or_a_current_file(self):
        fresh = layer(self.fields, [["LP-1", "One"]])
        self.assertEqual(rs.merge_kept(self.shape, fresh, None), (fresh, []))
        plain = rs.Shape(dataset="x", target="t", key="lp_number")
        self.assertEqual(rs.merge_kept(plain, fresh, layer(self.fields, [["LP-2", "Two"]]))[1], [])


class WriteTest(unittest.TestCase):
    def test_text_fields_are_as_wide_as_their_longest_value(self):
        specs = rs.compact([["name", "C", 254, 0], ["n", "N", 10, 0]], [["Café", 1], ["ab", 2]])
        self.assertEqual(specs, [["name", "C", 5, 0], ["n", "N", 10, 0]])

    def test_every_shape_is_configured_with_a_dataset_and_target(self):
        for name, shape in rs.SHAPES.items():
            self.assertRegex(shape.dataset, r"^[a-z0-9]{4}-[a-z0-9]{4}$", name)
            self.assertTrue(shape.target.endswith(Path(shape.target).name), name)

    def test_unknown_shape_is_refused(self):
        with self.assertRaises(SystemExit):
            rs.parse_args(["NoSuchShape"])


if __name__ == "__main__":
    unittest.main()
