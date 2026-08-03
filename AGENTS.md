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

## Project Skills

- For C# compiler warnings, analyzer diagnostics, static analysis, or analyzer
  policy, use the canonical project skill at
  `.agents/skills/net-roslyn-code-analyzers/SKILL.md`.
- Claude Code discovers the adapter under `.claude/skills/`; Codex discovers
  the canonical skill under `.agents/skills/`; GitHub Copilot follows the
  matching instruction under `.github/instructions/`.

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
