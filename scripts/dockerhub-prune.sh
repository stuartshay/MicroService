#!/usr/bin/env bash
# Deletes old Docker Hub tags of one repository, keeping "latest" and the newest KEEP tags
# (by last push). Docker Hub has no retention policy, so the deploy workflow runs this after
# each publish.
#
# Usage: scripts/dockerhub-prune.sh <namespace/repository> [keep]   (keep defaults to 5)
# Env:   DOCKERHUB_USERNAME, DOCKERHUB_PRUNE_TOKEN (a personal access token with Delete scope)
#        DRY_RUN=1 lists what would be deleted without deleting it.
# Without DOCKERHUB_PRUNE_TOKEN it prints a notice and exits 0, so a missing secret never
# fails a deploy.

set -Eeuo pipefail

readonly HUB="https://hub.docker.com/v2"

fail() {
    printf '[dockerhub-prune] ERROR: %s\n' "$*" >&2
    exit 1
}

log() {
    printf '[dockerhub-prune] %s\n' "$*"
}

repository="${1:-}"
keep="${2:-5}"
[[ "${repository}" == */* ]] || fail "Usage: $0 <namespace/repository> [keep]"
[[ "${keep}" =~ ^[1-9][0-9]*$ ]] || fail "keep must be a positive number, got '${keep}'"
command -v jq >/dev/null 2>&1 || fail "jq is required"

if [[ -z "${DOCKERHUB_PRUNE_TOKEN:-}" ]]; then
    log "DOCKERHUB_PRUNE_TOKEN is not set; skipping cleanup of ${repository}"
    exit 0
fi
: "${DOCKERHUB_USERNAME:?DOCKERHUB_USERNAME must be set}"

jwt="$(jq -n --arg u "${DOCKERHUB_USERNAME}" --arg p "${DOCKERHUB_PRUNE_TOKEN}" '{username: $u, password: $p}' |
    curl -fsS -H 'Content-Type: application/json' -d @- "${HUB}/users/login" | jq -r '.token')"
[[ -n "${jwt}" && "${jwt}" != "null" ]] || fail "Docker Hub login failed"

# Every tag with its last push time, newest first
tags_file="$(mktemp)"
trap 'rm -f "${tags_file}"' EXIT
url="${HUB}/repositories/${repository}/tags?page_size=100"
while [[ -n "${url}" && "${url}" != "null" ]]; do
    page="$(curl -fsS -H "Authorization: Bearer ${jwt}" "${url}")"
    jq -r '.results[] | "\(.last_updated)\t\(.name)"' <<<"${page}" >>"${tags_file}"
    url="$(jq -r '.next' <<<"${page}")"
done

mapfile -t delete < <(sort -r "${tags_file}" | cut -f2 | grep -vx 'latest' | tail -n "+$((keep + 1))")
total="$(wc -l <"${tags_file}")"
log "${repository}: ${total} tags, keeping latest and the newest ${keep}, deleting ${#delete[@]}"

# Docker Hub rate-limits its API: on 429, wait for Retry-After (capped) and try again.
# 404 means the tag is already gone.
delete_tag() {
    local tag="$1" attempt status wait headers
    headers="$(mktemp)"
    for attempt in 1 2 3 4 5; do
        status="$(curl -sS -o /dev/null -D "${headers}" -w '%{http_code}' -X DELETE \
            -H "Authorization: Bearer ${jwt}" "${HUB}/repositories/${repository}/tags/${tag}/")" || status=000
        case "${status}" in
            2??|404)
                rm -f "${headers}"
                return 0
                ;;
            429)
                wait="$(tr -d '\r' <"${headers}" | awk -F': *' 'tolower($1) == "retry-after" { print $2 }')"
                [[ "${wait}" =~ ^[0-9]+$ ]] || wait=30
                ((wait > 120)) && wait=120
                log "rate limited deleting ${tag}; waiting ${wait}s (attempt ${attempt})"
                sleep "${wait}"
                ;;
            *)
                break
                ;;
        esac
    done
    rm -f "${headers}"
    fail "could not delete ${tag} (HTTP ${status})"
}

for tag in "${delete[@]}"; do
    if [[ "${DRY_RUN:-0}" == "1" ]]; then
        log "would delete ${tag}"
    else
        delete_tag "${tag}"
    fi
done
log "done"
