---
applyTo: "**"
---

# Pull Request and Review Rules

These rules apply to every change proposed to this repository.

## Branch and Validation Requirements

1. Never commit or push directly to `master`.
2. Create a focused branch for each independently reviewable change.
3. Before publishing a pull request, run:
   - `make check`
   - `make build`
   - `make test`
   - `make analyze`
4. Resolve conflicts and verify that the intended diff contains no unrelated
   files or secrets.

## Creating Pull Requests

1. Create pull requests as **ready for review**, not as drafts, once local
   validation passes.
2. Include a concise summary, validation commands and results, dependency or
   stack information, and any known risks in the description.
3. A ready-for-review PR is not automatically merge-ready.
4. Do not enable auto-merge or add the PR to a merge queue during creation.

## Stacked Pull Requests

Use GitHub stacked pull requests when work depends on an unmerged change:

Reference: <https://docs.github.com/en/pull-requests/how-tos/stacked-pull-requests>

1. The bottom PR targets `master`.
2. Each higher PR targets the head branch of the PR immediately below it.
3. Each layer must contain one focused change and pass validation independently.
4. Link the PRs as a GitHub stack so default-branch rules and CI apply to every
   layer.
5. Put fixes in the layer where they logically belong, then perform a cascading
   rebase and rerun validation for affected layers.
6. Merge the top PR only when the entire stack satisfies the merge-readiness
   requirements below. GitHub will merge the stack from the bottom up.

## Codex Review Gate

The Codex review (`chatgpt-codex-connector`, automatic on every PR) is a
mandatory procedural gate. GitHub Copilot review is off for this repository.

1. Wait until Codex has posted its review summary for every PR in the stack.
2. Inspect every Codex finding (inline comments and the summary); do not treat
   an absent or pending review as approval.
3. For each actionable comment:
   - implement the fix in the correct stack layer;
   - run the relevant lint, build, and test commands;
   - push the fix;
   - reply directly to the review thread with what changed, the commit SHA, and
     the validation that passed;
   - request or wait for follow-up review when GitHub offers it.
4. Resolve a thread only after the concern is addressed or a documented,
   technically supported reason for not changing the code is accepted.
5. A commit that only fixes a finding needs no new Codex review. Re-check the
   review after a force-push or cascading rebase that changes other code.

## Merge-Readiness Checklist

Do not state that a PR or stack is merge-ready and do not merge it until all of
the following are true:

- [ ] The PR is ready for review and conflict-free.
- [ ] Required lint, build, and test checks pass on the current head SHA.
- [ ] Required code scanning checks pass on the current head SHA.
- [ ] Codex has completed its review on every affected PR.
- [ ] Every actionable review comment has been addressed and replied to.
- [ ] Required review threads are resolved.
- [ ] No newer commit or rebase invalidated the completed review or CI results.
- [ ] For a stack, every layer satisfies this checklist.

The final merge remains an explicit user or maintainer decision. Agents must
report the evidence and ask for confirmation before performing that irreversible
action unless the user has already explicitly authorized the merge.
