"""Regression checks for Docker Hub retention, without network access."""

import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest


SCRIPT = Path(__file__).with_name("dockerhub-prune.sh")
MOCK_CURL = '''#!/usr/bin/env python3
import datetime
import json
import os
from pathlib import Path
import sys

args = sys.argv[1:]
url = args[-1]
method = args[args.index("-X") + 1] if "-X" in args else "GET"
with open(os.environ["PRUNE_TEST_REQUESTS"], "a") as log:
    log.write(json.dumps({"method": method, "url": url}) + "\\n")
if url.endswith("/users/login"):
    sys.stdin.read()
    print(json.dumps({"token": "test-session"}))
    sys.exit(0)

case = os.environ["PRUNE_TEST_CASE"]
status = 200
body = {}
if method == "DELETE":
    status = 204
elif case in ("first_404", "first_403"):
    status = int(case[-3:])
elif "page=2" in url:
    if case == "stale_next":
        status = 404
    elif case == "later_503":
        status = 503
    else:
        body = {"results": json.loads(Path(os.environ["PRUNE_TEST_PAGE"]).read_text()),
                "next": None}
else:
    if case == "normal":
        count = 101
    else:
        count = 5
    records = [
        {"name": f"release-{i:03}",
         "last_updated": str(datetime.date(2026, 1, 1) + datetime.timedelta(days=i))}
        for i in reversed(range(count))
    ]
    records.insert(0, {"name": "latest", "last_updated": "2027-01-01"})
    body = {"count": 141, "results": records[:100],
            "next": "https://hub.docker.com/v2/repositories/test/repo/tags?page=2"}
    # Persist the normal second page between separate curl processes.
    Path(os.environ["PRUNE_TEST_PAGE"]).write_text(json.dumps(records[100:]))

if "-o" in args:
    Path(args[args.index("-o") + 1]).write_text(json.dumps(body))
if "-D" in args:
    Path(args[args.index("-D") + 1]).write_text("")
print(status, end="")
'''


class DockerHubPruneTest(unittest.TestCase):
    def run_case(self, case, dry_run=False):
        with tempfile.TemporaryDirectory(prefix="dockerhub-prune-test-") as directory:
            root = Path(directory)
            mock = root / "curl"
            mock.write_text(MOCK_CURL)
            mock.chmod(0o755)
            requests = root / "requests.jsonl"
            env = os.environ.copy()
            env.update(
                PATH=str(root) + os.pathsep + env["PATH"],
                DOCKERHUB_USERNAME="test-user",
                DOCKERHUB_PRUNE_TOKEN="test-token",
                PRUNE_TEST_CASE=case,
                PRUNE_TEST_REQUESTS=str(requests),
                PRUNE_TEST_PAGE=str(root / "page.json"),
                DRY_RUN="1" if dry_run else "0",
            )
            result = subprocess.run(
                ["bash", str(SCRIPT), "test/repo", "5"], env=env,
                text=True, capture_output=True, timeout=30,
            )
            calls = [json.loads(line) for line in requests.read_text().splitlines()]
            return result, calls

    def test_cached_next_page_404_is_end_of_listing(self):
        result, calls = self.run_case("stale_next")
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertIn("6 tags, keeping latest and the newest 5, deleting 0", result.stdout)
        self.assertFalse(any(call["method"] == "DELETE" for call in calls))

    def test_all_valid_pages_are_read_and_retention_is_preserved(self):
        result, calls = self.run_case("normal")
        self.assertEqual(0, result.returncode, result.stderr)
        deleted = {call["url"].rstrip("/").split("/")[-1]
                   for call in calls if call["method"] == "DELETE"}
        self.assertEqual({f"release-{i:03}" for i in range(96)}, deleted)

    def test_dry_run_never_deletes_tags(self):
        result, calls = self.run_case("normal", dry_run=True)
        self.assertEqual(0, result.returncode, result.stderr)
        self.assertEqual(96, result.stdout.count("would delete"))
        self.assertFalse(any(call["method"] == "DELETE" for call in calls))

    def test_first_page_errors_and_later_server_errors_fail_closed(self):
        for case in ("first_404", "first_403", "later_503"):
            with self.subTest(case=case):
                result, calls = self.run_case(case)
                self.assertNotEqual(0, result.returncode)
                self.assertIn("could not list tags", result.stderr)
                self.assertFalse(any(call["method"] == "DELETE" for call in calls))


if __name__ == "__main__":
    unittest.main()
