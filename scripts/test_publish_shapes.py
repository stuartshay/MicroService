"""Regression checks for publishing shapes to the bucket, without network access."""

import datetime
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
import zipfile


SCRIPT = Path(__file__).with_name("publish-shapes.py")
BUCKET = "files-shapes"
# A stand-in for gcloud: answers ls and rsync --dry-run from the case file, and logs every call
MOCK_GCLOUD = '''#!/usr/bin/env python3
import json
import os
import sys

args = sys.argv[1:]
with open(os.environ["PUBLISH_TEST_CALLS"], "a") as log:
    log.write(json.dumps(args) + "\\n")
case = json.load(open(os.environ["PUBLISH_TEST_CASE"]))
if args[:2] == ["storage", "ls"]:
    if args[2] == "--recursive":
        print("\\n".join(f"gs://files-shapes/{path}" for path in case["existing"]))
    else:
        if not case.get("backups"):
            sys.exit(1)
        print("\\n".join(f"gs://files-shapes/_previous/{name}/" for name in case["backups"]))
elif args[:2] == ["storage", "rsync"] and "--dry-run" in args:
    for path in case["copies"]:
        print(f"Would copy file://files/{path} to gs://files-shapes/{path}#1", file=sys.stderr)
    for path in case["removals"]:
        print(f"Would remove gs://files-shapes/{path}#1", file=sys.stderr)
    print("Would set mtime for gs://files-shapes/unchanged.shp#1", file=sys.stderr)
'''
SHAPE_PROPERTIES = '''
        [Shape("Borough_Boundaries/NAD83", "nybb", Datum.Nad83)]
        BoroughBoundaries,
        [Shape("LPC", "Buildings_20231209", Datum.Wgs84)]
        Buildings,
'''


class PublishShapesTest(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.dir = Path(self.temp.name)
        self.source = self.dir / "files"
        (self.source / "Borough_Boundaries/NAD83").mkdir(parents=True)
        (self.source / "Borough_Boundaries/NAD83/nybb.shp").write_bytes(b"shape")
        bin_dir = self.dir / "bin"
        bin_dir.mkdir()
        gcloud = bin_dir / "gcloud"
        gcloud.write_text(MOCK_GCLOUD)
        gcloud.chmod(0o755)
        self.calls = self.dir / "calls.jsonl"
        self.case = self.dir / "case.json"
        self.properties = self.dir / "ShapeProperties.cs"
        self.properties.write_text(SHAPE_PROPERTIES)
        self.env = dict(os.environ, PATH=f"{bin_dir}{os.pathsep}{os.environ['PATH']}",
                        PUBLISH_TEST_CALLS=str(self.calls), PUBLISH_TEST_CASE=str(self.case))
        self.env.pop("GITHUB_STEP_SUMMARY", None)

    def tearDown(self):
        self.temp.cleanup()

    def run_script(self, *args, existing=(), copies=(), removals=(), backups=()):
        self.case.write_text(json.dumps({"existing": list(existing), "copies": list(copies),
                                         "removals": list(removals), "backups": list(backups)}))
        if self.calls.exists():
            self.calls.unlink()
        return subprocess.run([sys.executable, str(SCRIPT), *args], capture_output=True,
                              text=True, env=self.env)

    def publish(self, *extra, **case):
        return self.run_script("publish", "--source", str(self.source), "--bucket", BUCKET,
                               "--backup-prefix", "_previous/20261101-abc1234",
                               "--shape-properties", str(self.properties), *extra, **case)

    def gcloud_calls(self):
        if not self.calls.exists():
            return []
        return [json.loads(line) for line in self.calls.read_text().splitlines()]

    def writes(self):
        return [call for call in self.gcloud_calls()
                if call[:2] in (["storage", "cp"], ["storage", "rm"])
                or (call[:2] == ["storage", "rsync"] and "--dry-run" not in call)]

    def test_backs_up_replaced_and_removed_objects_before_the_sync(self):
        result = self.publish(existing=["a/old.dbf", "a/gone.txt"],
                              copies=["a/old.dbf", "a/new.shp"], removals=["a/gone.txt"],
                              )
        self.assertEqual(result.returncode, 0, result.stderr)
        writes = self.writes()
        self.assertEqual(writes[0], ["storage", "cp", "gs://files-shapes/a/gone.txt",
                                     "gs://files-shapes/_previous/20261101-abc1234/a/gone.txt"])
        self.assertEqual(writes[1], ["storage", "cp", "gs://files-shapes/a/old.dbf",
                                     "gs://files-shapes/_previous/20261101-abc1234/a/old.dbf"])
        self.assertEqual(writes[2][:2], ["storage", "rsync"])
        self.assertEqual(len(writes), 3, "a new object needs no backup")
        self.assertIn("1 added, 1 replaced, 1 removed", result.stdout)

    def test_sync_keeps_backups_and_zips_out_of_the_mirror(self):
        self.publish(copies=["a/new.shp"])
        sync = self.writes()[-1]
        self.assertIn("--delete-unmatched-destination-objects", sync)
        self.assertIn("--checksums-only", sync)
        excludes = [sync[i + 1] for i, arg in enumerate(sync) if arg == "--exclude"]
        self.assertEqual(excludes, [r"^_previous/", r".*\.dbf\.zip$"])

    def test_no_changes_means_no_writes(self):
        result = self.publish(existing=["a/same.shp"])
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(self.writes(), [])
        self.assertIn("already matches", result.stdout)

    def test_refuses_to_delete_a_file_a_configured_shape_reads(self):
        result = self.publish(existing=["Borough_Boundaries/NAD83/nybb.dbf"],
                              removals=["Borough_Boundaries/NAD83/nybb.dbf"])
        self.assertEqual(result.returncode, 1)
        self.assertIn("configured shape reads: Borough_Boundaries/NAD83/nybb.dbf", result.stderr)
        self.assertEqual(self.writes(), [], "nothing may change when the guard stops")

    def test_refuses_too_many_deletions_that_git_did_not_make(self):
        strays = [f"stray/{n}.txt" for n in range(3)]
        result = self.publish("--max-stray-deletions", "2", existing=strays, removals=strays)
        self.assertEqual(result.returncode, 1)
        self.assertIn("3 objects that were not deleted in git (limit 2)", result.stderr)
        self.assertEqual(self.writes(), [])

    def test_deletions_made_in_git_do_not_count_against_the_limit(self):
        removed = [f"Archive/{n}.shp" for n in range(5)] + ["LPC/Old_20231209.dbf.zip"]
        deleted = self.dir / "deleted.txt"
        deleted.write_text("\n".join(removed) + "\n")
        # The unzipped .dbf of a deleted .dbf.zip counts as deleted in git too
        result = self.publish("--max-stray-deletions", "0", "--git-deleted", str(deleted),
                              existing=removed[:5] + ["LPC/Old_20231209.dbf"],
                              removals=removed[:5] + ["LPC/Old_20231209.dbf"])
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(len([w for w in self.writes() if w[:2] == ["storage", "cp"]]), 6)

    def test_allow_deletions_overrides_the_guard(self):
        result = self.publish("--allow-deletions", existing=["Borough_Boundaries/NAD83/nybb.dbf"],
                              removals=["Borough_Boundaries/NAD83/nybb.dbf"])
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(self.writes()[-1][:2], ["storage", "rsync"])

    def test_dry_run_changes_nothing(self):
        result = self.publish("--dry-run", existing=["a/old.dbf"], copies=["a/old.dbf"],
                              removals=["a/gone.txt"])
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(self.writes(), [])
        self.assertIn("dry run", result.stdout)

    def test_backup_prefix_must_be_under_previous(self):
        result = self.run_script("publish", "--source", str(self.source), "--bucket", BUCKET,
                                 "--backup-prefix", "Borough_Boundaries")
        self.assertEqual(result.returncode, 1)
        self.assertIn("must start with _previous/", result.stderr)

    def test_unzip_extracts_a_large_dbf_beside_its_zip(self):
        folder = self.source / "LPC"
        folder.mkdir()
        with zipfile.ZipFile(folder / "Buildings_20231209.dbf.zip", "w") as zipped:
            zipped.writestr("Buildings_20231209.dbf", b"dbf bytes")
        result = self.run_script("unzip", "--source", str(self.source))
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual((folder / "Buildings_20231209.dbf").read_bytes(), b"dbf bytes")

    def test_unzip_rejects_a_zip_with_other_names(self):
        folder = self.source / "LPC"
        folder.mkdir()
        with zipfile.ZipFile(folder / "Buildings_20231209.dbf.zip", "w") as zipped:
            zipped.writestr("geo_export_123.dbf", b"dbf bytes")
        result = self.run_script("unzip", "--source", str(self.source))
        self.assertEqual(result.returncode, 1)
        self.assertIn("must hold only Buildings_20231209.dbf", result.stderr)

    def test_prune_deletes_only_backups_older_than_the_limit(self):
        today = datetime.datetime.now(datetime.timezone.utc).date()
        old = (today - datetime.timedelta(days=91)).strftime("%Y%m%d") + "-aaaaaaa"
        recent = (today - datetime.timedelta(days=89)).strftime("%Y%m%d") + "-bbbbbbb"
        result = self.run_script("prune", "--bucket", BUCKET, "--keep-days", "90",
                                 backups=[old, recent, "not-a-backup"])
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual([w for w in self.writes() if w[:2] == ["storage", "rm"]],
                         [["storage", "rm", "--recursive", f"gs://files-shapes/_previous/{old}/"]])


if __name__ == "__main__":
    unittest.main()
