---
name: net-roslyn-code-analyzers
description: Diagnose, configure, and remediate .NET Roslyn analyzer diagnostics in this repository. Use for C# compiler or analyzer warnings, CA/IDE/S/SA/RCS diagnostic codes, static-analysis failures, code-quality gates, .editorconfig severity changes, or requests to run and interpret the repository's analyzer workflow. Do not use for ordinary feature work without analyzer findings.
---

# .NET Roslyn Code Analyzers

Apply analyzer changes incrementally while preserving application behavior.

## Workflow

1. Read `Makefile`, `Directory.Build.props`, relevant project files, and any
   `.editorconfig` before changing analyzer configuration.
2. Reproduce the finding with the narrowest applicable command:
   - `dotnet build <project> --no-restore` for a single project.
   - `make analyze` for a solution-wide analyzer build.
   - `make sonar` only when SonarQube credentials and server access are
     available and the user requests server analysis.
3. Record each diagnostic ID, severity, file, and line. Do not infer a rule from
   message text when an ID is available.
4. Classify the finding before editing. Read
   [diagnostic-routing.md](references/diagnostic-routing.md) for routing and
   remediation priorities.
5. Fix correctness, security, reliability, and performance findings before
   maintainability or style findings. Keep each change focused.
6. Prefer correcting code. Change severity or suppress a diagnostic only when
   the rule is inapplicable, generated code is involved, or compatibility
   requires it. Use the narrowest suppression and document why.
7. Run formatting only on touched files. Avoid broad analyzer-driven rewrites.
8. Validate changes with `make check`, `make analyze`, `make build`, and
   `make test` so the PR has evidence matching the named CI checks.
9. Report fixed diagnostic IDs, intentional suppressions, commands run, and any
   remaining findings.

## Analyzer policy

- Treat existing SDK analyzers and configured Sonar analysis as the current
  baseline.
- Do not add Roslynator, StyleCop, SonarAnalyzer, or another analyzer package
  without checking overlap, expected warning volume, maintenance status, and
  CI impact.
- Introduce a new analyzer suite in its own focused PR with an explicit baseline
  and staged severity policy.
- Keep build and local behavior aligned; do not create an agent-only quality
  gate that contributors or CI cannot reproduce.

## Boundaries

- Do not weaken rules globally merely to obtain a clean build.
- Do not mix unrelated refactoring or package upgrades into a diagnostic fix.
- Never run `make sonar` without the required local credentials.
- Do not expose `.env`, Sonar tokens, or scanner-generated output.
