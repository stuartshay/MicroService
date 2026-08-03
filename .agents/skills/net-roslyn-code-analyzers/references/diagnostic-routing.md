# Diagnostic routing

Use the diagnostic prefix and rule documentation to choose the remediation
path. Confirm unfamiliar rules in the analyzer owner's official documentation.

| Prefix | Typical source | First action |
| --- | --- | --- |
| `CS` | C# compiler | Fix correctness or language semantics before analyzer work. |
| `CA` | .NET SDK analyzers | Check correctness, security, reliability, and performance impact. |
| `IDE` | .NET code style | Confirm repository style policy before changing severity. |
| `S` | Sonar analyzer/server | Confirm the issue and quality profile in SonarQube. |
| `SA` | StyleCop | Check style configuration and avoid documentation churn. |
| `RCS` | Roslynator | Confirm that Roslynator is installed before applying its guidance. |

## Priority

1. Correctness and security
2. Reliability and resource lifetime
3. Performance
4. Maintainability
5. Naming, layout, and documentation style

## Suppression decision

Suppress only when at least one condition is true:

- The analyzer cannot understand a valid framework or generated-code pattern.
- A public compatibility contract prevents the recommended change.
- The finding is a documented false positive.
- A broader fix belongs to a separately tracked change and the temporary
  suppression has an owner and explanation.

Prefer, in order: a code fix, a scoped source suppression with justification, a
project-specific severity entry, then a repository-wide policy change.
