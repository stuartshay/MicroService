#!/usr/bin/env bash
# Smoke check after a deploy: one known lookup per shape a client uses.
# A shape that failed to load, or a publish that broke a file, changes these answers.
#
# Usage: scripts/shape-smoke-check.sh <base-url>
#   e.g. scripts/shape-smoke-check.sh https://microservice-api-w6zlqlyoma-uk.a.run.app
set -euo pipefail

base="${1:?usage: $0 <base-url>}"
lookup="${base%/}/api/v1/FeatureService/geospatiallookup"
failures=0

# check <shape> <x> <y> <datum> <jq field> <expected> <place>
check() {
  local shape="$1" x="$2" y="$3" datum="$4" field="$5" expected="$6" place="$7" body actual
  if ! body="$(curl --fail --silent --show-error --max-time 30 --retry 3 --retry-delay 5 \
    "${lookup}?X=${x}&Y=${y}&Datum=${datum}&Type=${shape}")"; then
    echo "FAIL ${shape} at ${place}: request failed" >&2
    failures=$((failures + 1))
    return
  fi
  actual="$(jq -r "${field} // empty" <<<"${body}")"
  if [[ "${actual}" == "${expected}" ]]; then
    echo "ok   ${shape} at ${place}: ${actual}"
  else
    echo "FAIL ${shape} at ${place}: expected ${field} = ${expected}, got '${actual}'" >&2
    failures=$((failures + 1))
  fi
}

check BoroughBoundaries -73.98566 40.74844 Wgs84 .boroName Manhattan "Empire State Building"
check IndividualLandmarkSite -73.98566 40.74844 Wgs84 .lpNumber LP-02000 "Empire State Building"
check IndividualLandmarkHistoricDistricts -73.97723 40.75273 Wgs84 .bbl 1012800001 "Grand Central Terminal"
check HistoricDistricts -73.99560 40.69590 Wgs84 .lpNumber LP-00099 "Brooklyn Heights"
check ScenicLandmarks -73.96541 40.78260 Wgs84 .lpNumber LP-00851 "Central Park"
check Subway 1021192.94 212550.02 Nad83 .name "Junction Blvd & Roosevelt Ave at SE corner" "Junction Blvd (NAD83)"

if ((failures > 0)); then
  echo "${failures} shape lookup(s) failed" >&2
  exit 1
fi
echo "All shape lookups passed"
