---
applyTo: "src/**/*.cs,test/**/*.cs,**/*.csproj,Directory.Build.props,.editorconfig"
---

# .NET Roslyn Code Analyzers

When a request is related to C# warnings, analyzer diagnostics, code quality
policy, linting, or rule remediation:

1. Use the shared project skill in
   `.agents/skills/net-roslyn-code-analyzers/SKILL.md`.
2. Prefer repository commands for validation:
   - `make check`
   - `make analyze`
   - `make test`
   - `make sonar` when SonarQube findings are in scope
3. Keep analyzer fixes scoped to the reported diagnostics and avoid unrelated
   refactors.
4. If analyzer severity changes are needed, update policy intentionally and
   explain impact on CI and local development.
