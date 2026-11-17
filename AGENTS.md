# Agent Operating Guide

All automation, assistants, and developers must follow the project instructions
under `.github/instructions/`.

## Project Commands

- Setup: `make setup`
- Restore and build: `make build`
- Test: `make test`
- Run all local checks: `make check`
- Run analyzer validation: `make analyze`
- Run pre-commit directly: `pre-commit run --all-files`

## Project Layout

| Path | Role |
| --- | --- |
| `src/MicroService.WebApi` | Controllers, OpenAPI (`Extensions/OpenApi`), health checks, DI in `Program.cs`, and the image's `Dockerfile` |
| `src/MicroService.Service` | Shape and flat-file services, `ShapeRegistry`, mappings, configuration |
| `src/MicroService.Common` | Logging, health check helpers and their configuration |
| `src/MicroService.Data` | Shared enums |
| `test/MicroService.Test` | xUnit v3 tests, including contract tests and dataset snapshots |
| `files/` | The shapefiles and CSVs (source of truth; see `files/README.md`) |
| `scripts/` | Shape publishing, the post-deploy smoke check, Docker Hub pruning, setup |

## Deployment

- Merging to `master` is a production deploy when it changes `src/`, `test/`,
  `files/`, the build files or `.github/workflows/actions.yml`. Docs-only changes
  don't deploy.
- The deploy publishes `files/` to the `files-shapes` bucket first, then deploys
  Cloud Run `microservice-api` (us-east4) with the bucket mounted read-only, HTTP
  probes on `/health` and no minimum instances, and runs the smoke check
  (`scripts/shape-smoke-check.sh`).
- After a deploying merge, confirm the new revision serves 100% of traffic and that
  `/health`, the smoke check and the LP-00099 contract check pass
  (`docs/improvement-plan.md`, Validation).
- `POST /api/v1/FeatureService/attributelookup` and `attributelookupgeojson` are a
  fixed contract with CoreDataStore; change them only together with it.
- Do not change production (Cloud Run settings, the bucket, IAM) outside the deploy
  workflow without the owner's approval.

## Analyzer findings

- Reproduce a C# warning or analyzer diagnostic with `make analyze` (warnings as
  errors, as in CI) and record its ID before changing anything.
- Fix the code first. Suppress a rule only when it doesn't apply, with the
  narrowest scope and a comment that says why; don't weaken rules globally.
- Keep analyzer fixes out of unrelated changes, and validate with `make check`,
  `make analyze`, `make build` and `make test`.

## Pull Request Workflow

Follow `.github/instructions/pull-requests.instructions.md` for every pull
request. In particular:

- Stacked PR reference: `https://docs.github.com/en/pull-requests/how-tos/stacked-pull-requests`

- Never commit directly to `master`.
- Create focused branches and pull requests that are ready for review, not
  drafts, after local validation passes.
- Use a GitHub stacked pull request when a change depends on an unmerged pull
  request. Keep the bottom PR based on `master` and each higher PR based on the
  branch immediately below it.
- Ensure required CI checks, including CodeQL code scanning, pass on the current
  head SHA before considering a PR merge-ready.
- Do not describe a PR or stack as merge-ready, enable auto-merge, add it to a
  merge queue, or merge it until Codex has completed its review.
- Address every actionable Codex finding, reply with the resolution and
  validation evidence, and wait for any requested follow-up review.
- A PR is merge-ready only when required CI passes, Codex review is complete,
  actionable review threads are resolved, and the PR is conflict-free.

## Safety

- Do not commit `.env`, credentials, tokens, or generated secrets.
- Preserve unrelated working-tree changes and explicitly stage only files that
  belong to the current change.
- Use `--force-with-lease` only when a stack rebase requires rewriting a
  feature branch; never force-push `master`.
