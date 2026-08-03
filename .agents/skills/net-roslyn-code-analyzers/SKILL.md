---
name: net-roslyn-code-analyzers
description: >-
  Diagnose and remediate Roslyn compiler and analyzer findings in this .NET
  solution. Use for C# warnings, CA/CS/IDE diagnostics, analyzer policy changes,
  code-quality gates, or warning-focused cleanup work.
---

# Roslyn Analyzer Workflow

## Repository Context

- Solution: MicroService.sln
- Build commands:
  - make check
  - make analyze
  - make build
  - make test
  - make sonar
- Shared build policy file: Directory.Build.props

## Workflow

1. Reproduce diagnostics with `make analyze` or `make sonar`.
2. Classify diagnostics by scope:
   - correctness or security
   - maintainability or style
   - performance
3. Confirm the affected behavior and add regression coverage when appropriate.
4. Fix the root cause without unrelated refactoring or broad suppression.
5. Run `make check`, `make analyze`, and `make test`.
6. Run `make sonar` when SonarQube findings or metrics are in scope.
7. Report rule-count changes and any intentionally deferred findings.

## Common Actions

- Correct API misuse and nullability warnings.
- Address style and readability diagnostics without broad rewrites.
- Tighten or relax rule severity only with explicit justification.
- Keep behavior stable while fixing analyzer findings.
- Keep each pull request focused on a small, independently validated rule set.

## Non-Goals

- Do not perform unrelated refactors while resolving analyzer diagnostics.
- Do not weaken rules globally unless explicitly requested.
- Do not add null-forgiving operators or suppressions without proving the value
  is safe at that boundary.

## References

- https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-options
- https://learn.microsoft.com/dotnet/fundamentals/code-analysis/code-style-rule-options
