# Cleanup Plan

All phases are complete; see the [archive](archive/cleanup-plan-2026-11.md) for
phases 1 to 4 (#531, #533, #535, #536). Only owner steps and dependency
upgrades are left. The [improvement plan](improvement-plan.md) is also complete.

## Owner steps

**Agent**: repository-only work in one pull request that stops for review.
**Claude**: production and deploy work, and reviewing agent pull requests.
**Owner**: decisions, repository settings and credentials.

| Item | Owner | Status |
| --- | --- | --- |
| GitHub Pages | Owner | Turn Pages off (settings; status unverified). `docfx/` and `docfx.yml` are gone, and `origin` no longer has a `gh-pages` branch |
| Local `credentials/gcp-gh-actions-navigator.json` and its disabled key `d3b8e69e` | Owner | Delete the file and the key in IAM (status unverified; local to the owner's machine) |

## Next

| Item | Who | Blockers |
| --- | --- | --- |
| Major upgrades: .NET 11 (one change: SDK, target framework, images, packages), Loki 3, Grafana 13, Prometheus 3 | Agent, then Claude verifies the deploy | Owner approves each in Renovate's Dependency Dashboard; `renovate/major-docker-images` is open on `origin` |
| `scripts/update_cloud_run_service.sh`: keep as the documented way to recreate the `files-shapes` mount, or move the mount into Terraform and remove it (status unverified: the script is still present and nothing documents it) | Owner decides, then Agent | Owner decision |

## Later

Nothing.

## Validation for every pull request

`make check`, `make build`, `make test` and `make analyze` pass; the Copilot
review is answered and every thread resolved; after a deploying merge the
contract check passes (see the [improvement plan](improvement-plan.md)) and the
latest revision serves 100% of traffic.
