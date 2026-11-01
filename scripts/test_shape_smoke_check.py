"""Regression checks for the post-deploy shape smoke check, without network access."""

import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest
from urllib.parse import parse_qs, urlparse


SCRIPT = Path(__file__).with_name("shape-smoke-check.sh")
BASE_URL = "https://microservice.test"
# What production answers today, by shape
ANSWERS = {
    "BoroughBoundaries": {"boroName": "Manhattan"},
    "IndividualLandmarkSite": {"lpNumber": "LP-02000"},
    "IndividualLandmarkHistoricDistricts": {"bbl": "1012800001"},
    "HistoricDistricts": {"lpNumber": "LP-00099"},
    "ScenicLandmarks": {"lpNumber": "LP-00851"},
    "Subway": {"name": "Junction Blvd & Roosevelt Ave at SE corner"},
}
# A stand-in for curl: answers each lookup from the case file, and logs every URL
MOCK_CURL = '''#!/usr/bin/env python3
import json
import os
import sys
from urllib.parse import parse_qs, urlparse

url = sys.argv[-1]
with open(os.environ["SMOKE_TEST_URLS"], "a") as log:
    log.write(url + "\\n")
shape = parse_qs(urlparse(url).query)["Type"][0]
case = json.load(open(os.environ["SMOKE_TEST_CASE"]))
if shape in case["failing"]:
    print("curl: (22) The requested URL returned error: 404", file=sys.stderr)
    sys.exit(22)
print(json.dumps(case["answers"][shape]))
'''


class ShapeSmokeCheckTest(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.dir = Path(self.temp.name)
        bin_dir = self.dir / "bin"
        bin_dir.mkdir()
        curl = bin_dir / "curl"
        curl.write_text(MOCK_CURL)
        curl.chmod(0o755)
        self.urls = self.dir / "urls.txt"
        self.case = self.dir / "case.json"
        self.env = dict(os.environ, PATH=f"{bin_dir}{os.pathsep}{os.environ['PATH']}",
                        SMOKE_TEST_URLS=str(self.urls), SMOKE_TEST_CASE=str(self.case))

    def tearDown(self):
        self.temp.cleanup()

    def run_script(self, *args, answers=None, failing=()):
        self.case.write_text(json.dumps({"answers": answers or ANSWERS, "failing": list(failing)}))
        return subprocess.run(["bash", str(SCRIPT), *args], capture_output=True, text=True,
                              env=self.env)

    def requested(self):
        return [parse_qs(urlparse(url).query) for url in self.urls.read_text().splitlines()]

    def test_passes_when_every_shape_answers_as_expected(self):
        result = self.run_script(BASE_URL)
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("All shape lookups passed", result.stdout)
        self.assertEqual(sorted(query["Type"][0] for query in self.requested()), sorted(ANSWERS))

    def test_lookups_go_to_the_given_base_url(self):
        self.run_script(BASE_URL + "/")
        url = self.urls.read_text().splitlines()[0]
        self.assertTrue(url.startswith(f"{BASE_URL}/api/v1/FeatureService/geospatiallookup?"), url)

    def test_fails_on_a_wrong_answer_and_names_the_shape(self):
        answers = dict(ANSWERS, ScenicLandmarks={"lpNumber": "LP-00901"})
        result = self.run_script(BASE_URL, answers=answers)
        self.assertEqual(result.returncode, 1)
        self.assertIn("FAIL ScenicLandmarks at Central Park: expected .lpNumber = LP-00851, got 'LP-00901'",
                      result.stderr)
        self.assertIn("1 shape lookup(s) failed", result.stderr)

    def test_a_missing_field_counts_as_a_failure(self):
        answers = dict(ANSWERS, BoroughBoundaries={})
        result = self.run_script(BASE_URL, answers=answers)
        self.assertEqual(result.returncode, 1)
        self.assertIn("FAIL BoroughBoundaries at Empire State Building: expected .boroName = Manhattan, got ''",
                      result.stderr)

    def test_a_failed_request_fails_but_the_other_shapes_are_still_checked(self):
        result = self.run_script(BASE_URL, failing=["Subway"])
        self.assertEqual(result.returncode, 1)
        self.assertIn("FAIL Subway at Junction Blvd (NAD83): request failed", result.stderr)
        self.assertEqual(len(self.requested()), len(ANSWERS))

    def test_subway_is_checked_in_nad83(self):
        self.run_script(BASE_URL)
        subway = next(query for query in self.requested() if query["Type"] == ["Subway"])
        self.assertEqual(subway["Datum"], ["Nad83"])

    def test_requires_a_base_url(self):
        result = self.run_script()
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("usage:", result.stderr)


if __name__ == "__main__":
    unittest.main()
