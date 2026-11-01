#!/usr/bin/env python3
"""Publish files/ to the shapes bucket that Cloud Run mounts.

Git is the source of truth: the bucket is made to match files/, and every
object that is replaced or deleted is first copied to a backup prefix
(_previous/<date>-<commit>/), because the bucket keeps no object versions.

Commands:
  unzip     Extract each <name>.dbf.zip next to itself. A .dbf over GitHub's
            100 MB limit is committed zipped; CI, make test and publish use this.
  publish   Unzip, back up, then sync files/ to the bucket.
  prune     Delete backups older than --keep-days.

publish stops before changing anything if the sync would delete a file a
shape in ShapeProperties.cs reads, or more than --max-stray-deletions objects
that were not deleted in git (pass --git-deleted), unless --allow-deletions.
"""

import argparse
import datetime
import os
from pathlib import Path
import re
import subprocess
import sys
import zipfile

ROOT = Path(__file__).resolve().parent.parent
SHAPE_PROPERTIES = ROOT / "src/MicroService.Service/Models/Enum/ShapeProperties.cs"
SHAPE_PARTS = (".shp", ".shx", ".dbf", ".prj", ".cpg")
BACKUP_ROOT = "_previous"
# The zips that publish extracts stay out of the sync. gcloud matches each pattern from
# the start of the relative path (re.match), and only against the source: an exclude
# does not stop --delete-unmatched-destination-objects deleting bucket objects. So the
# sync never deletes; publish removes the planned deletions itself, skipping backups.
EXCLUDES = (r".*\.dbf\.zip$",)


def log(message):
    print(f"[publish-shapes] {message}", flush=True)


def fail(message):
    print(f"[publish-shapes] ERROR: {message}", file=sys.stderr, flush=True)
    sys.exit(1)


def gcloud(*args, check=True):
    result = subprocess.run(["gcloud", *args], capture_output=True, text=True)
    if check and result.returncode != 0:
        fail(f"gcloud {' '.join(args[:3])} failed:\n{result.stderr.strip()}")
    return result


def unzip(source):
    """Extract every <name>.dbf.zip into <name>.dbf beside it."""
    extracted = []
    for archive in sorted(Path(source).rglob("*.dbf.zip")):
        target = archive.with_suffix("")  # drops .zip
        with zipfile.ZipFile(archive) as zipped:
            names = zipped.namelist()
            if names != [target.name]:
                fail(f"{archive} must hold only {target.name}, found {names}")
            with zipped.open(target.name) as data, open(target, "wb") as out:
                while chunk := data.read(1 << 20):
                    out.write(chunk)
        extracted.append(target)
        log(f"extracted {target.relative_to(source)}")
    return extracted


def configured_files(path=SHAPE_PROPERTIES):
    """Every file a configured shape reads, as bucket-relative paths."""
    text = Path(path).read_text(encoding="utf-8")
    pairs = re.findall(r'\[Shape\("([^"]+)",\s*"([^"]+)"', text)
    if not pairs:
        fail(f"no [Shape(...)] attributes found in {path}")
    return {f"{folder}/{name}{part}" for folder, name in pairs for part in SHAPE_PARTS}


def rsync_args(source, bucket, dry_run):
    """The dry run plans deletions too; the real sync only copies."""
    args = ["storage", "rsync", "--recursive", "--checksums-only"]
    for pattern in EXCLUDES:
        args += ["--exclude", pattern]
    if dry_run:
        args += ["--delete-unmatched-destination-objects", "--dry-run"]
    return args + [str(source), f"gs://{bucket}"]


def object_path(url, bucket):
    """gs://bucket/a/b.shp#123 -> a/b.shp"""
    prefix = f"gs://{bucket}/"
    path = url[len(prefix):] if url.startswith(prefix) else url
    return re.sub(r"#\d+$", "", path)


def plan(source, bucket):
    """What a sync would copy and remove, from gcloud's dry run."""
    result = gcloud(*rsync_args(source, bucket, dry_run=True))
    copies, removals = [], []
    for line in (result.stdout + result.stderr).splitlines():
        if match := re.match(r"Would copy \S.* to (gs://.+)$", line):
            copies.append(object_path(match.group(1), bucket))
        elif match := re.match(r"Would (?:remove|delete) (gs://.+)$", line):
            path = object_path(match.group(1), bucket)
            if not path.startswith(f"{BACKUP_ROOT}/"):
                removals.append(path)
    return copies, removals


def existing_objects(bucket):
    result = gcloud("storage", "ls", "--recursive", f"gs://{bucket}/**")
    return {object_path(line.strip(), bucket) for line in result.stdout.splitlines()
            if line.startswith(f"gs://{bucket}/")}


def read_git_deleted(path):
    """Paths deleted in git (relative to files/); a deleted x.dbf.zip also covers x.dbf."""
    if not path:
        return set()
    deleted = {line.strip() for line in Path(path).read_text().splitlines() if line.strip()}
    return deleted | {name[:-len(".zip")] for name in deleted if name.endswith(".dbf.zip")}


def check_deletions(removals, configured, git_deleted, max_stray, allow):
    """Problems that stop the publish, unless deletions are allowed."""
    problems = []
    shape_files = sorted(set(removals) & configured)
    if shape_files:
        problems.append("would delete files a configured shape reads: " + ", ".join(shape_files))
    strays = sorted(set(removals) - git_deleted)
    if len(strays) > max_stray:
        problems.append(f"would delete {len(strays)} objects that were not deleted in git "
                        f"(limit {max_stray}): " + ", ".join(strays[:10]))
    if problems and not allow:
        fail("; ".join(problems) + ". Run the workflow by hand with allow_deletions to proceed.")
    return problems


def summary(lines):
    log("\n  ".join(lines))
    if step_summary := os.environ.get("GITHUB_STEP_SUMMARY"):
        with open(step_summary, "a", encoding="utf-8") as out:
            out.write("### Shapes published\n\n" + "\n".join(f"- {line}" for line in lines) + "\n")


def publish(args):
    source = Path(args.source)
    if not source.is_dir():
        fail(f"{source} is not a directory")
    unzip(source)
    copies, removals = plan(source, args.bucket)
    existing = existing_objects(args.bucket)
    replaced = sorted(path for path in copies if path in existing)
    added = sorted(path for path in copies if path not in existing)
    check_deletions(removals, configured_files(args.shape_properties),
                    read_git_deleted(args.git_deleted), args.max_stray_deletions,
                    args.allow_deletions)

    backups = sorted(set(replaced) | set(removals))
    lines = [f"{len(added)} added, {len(replaced)} replaced, {len(removals)} removed",
             f"backup: gs://{args.bucket}/{args.backup_prefix}/ ({len(backups)} objects)"]
    lines += [f"added {path}" for path in added]
    lines += [f"replaced {path}" for path in replaced]
    lines += [f"removed {path}" for path in removals]
    if args.dry_run:
        summary(["dry run, nothing changed"] + lines)
        return

    for path in backups:
        gcloud("storage", "cp", f"gs://{args.bucket}/{path}",
               f"gs://{args.bucket}/{args.backup_prefix}/{path}")
    if copies:
        gcloud(*rsync_args(source, args.bucket, dry_run=False))
    if removals:
        gcloud("storage", "rm", *(f"gs://{args.bucket}/{path}" for path in removals))
    summary(lines if copies or removals else ["no changes: the bucket already matches files/"])


def prune(args):
    """Delete _previous/<YYYYMMDD>-<commit>/ backups older than --keep-days."""
    result = gcloud("storage", "ls", f"gs://{args.bucket}/{BACKUP_ROOT}/", check=False)
    if result.returncode != 0:
        log("no backups to prune")
        return
    cutoff = datetime.datetime.now(datetime.timezone.utc).date() - datetime.timedelta(days=args.keep_days)
    for line in result.stdout.splitlines():
        match = re.search(rf"/{BACKUP_ROOT}/(\d{{8}})-[^/]+/$", line.strip())
        if not match:
            continue
        taken = datetime.datetime.strptime(match.group(1), "%Y%m%d").date()
        if taken < cutoff:
            gcloud("storage", "rm", "--recursive", line.strip())
            log(f"pruned {line.strip()}")


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    commands = parser.add_subparsers(dest="command", required=True)

    unzip_cmd = commands.add_parser("unzip", help="extract <name>.dbf.zip files")
    unzip_cmd.add_argument("--source", default=str(ROOT / "files"))

    publish_cmd = commands.add_parser("publish", help="sync files/ to the bucket")
    publish_cmd.add_argument("--source", default=str(ROOT / "files"))
    publish_cmd.add_argument("--bucket", required=True)
    publish_cmd.add_argument("--backup-prefix", required=True,
                             help=f"for example {BACKUP_ROOT}/20261101-abc1234")
    publish_cmd.add_argument("--git-deleted", help="file listing paths deleted in git, relative to files/")
    publish_cmd.add_argument("--max-stray-deletions", type=int, default=20)
    publish_cmd.add_argument("--allow-deletions", action="store_true")
    publish_cmd.add_argument("--dry-run", action="store_true")
    publish_cmd.add_argument("--shape-properties", default=str(SHAPE_PROPERTIES))

    prune_cmd = commands.add_parser("prune", help="delete old backups")
    prune_cmd.add_argument("--bucket", required=True)
    prune_cmd.add_argument("--keep-days", type=int, default=90)

    args = parser.parse_args(argv)
    if args.command == "unzip":
        unzip(Path(args.source))
    elif args.command == "publish":
        if not args.backup_prefix.startswith(f"{BACKUP_ROOT}/"):
            fail(f"--backup-prefix must start with {BACKUP_ROOT}/")
        publish(args)
    else:
        prune(args)


if __name__ == "__main__":
    main()
