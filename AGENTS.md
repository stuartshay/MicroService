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
  merge queue, or merge it until GitHub Copilot has completed its review.
- Address every actionable Copilot comment, reply with the resolution and
  validation evidence, and wait for any requested follow-up review.
- A PR is merge-ready only when required CI passes, Copilot review is complete,
  actionable review threads are resolved, and the PR is conflict-free.

## Safety

- Do not commit `.env`, credentials, tokens, or generated secrets.
- Preserve unrelated working-tree changes and explicitly stage only files that
  belong to the current change.
- Use `--force-with-lease` only when a stack rebase requires rewriting a
  feature branch; never force-push `master`.
