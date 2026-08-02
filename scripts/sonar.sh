#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
readonly SCRIPT_DIR
REPO_ROOT="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly REPO_ROOT
readonly ENV_FILE="${REPO_ROOT}/.env"
readonly PROJECT_KEY="MicroService.Api"
readonly SOLUTION="MicroService.sln"
readonly TEST_PROJECT="test/MicroService.Test/MicroService.Test.csproj"
readonly RESULTS_DIRECTORY=".test-results/sonar"

fail() {
    printf '[sonar] ERROR: %s\n' "$*" >&2
    exit 1
}

[[ -f "${ENV_FILE}" ]] || fail "Missing ${ENV_FILE}. Define SONAR_HOST_URL and SONAR_TOKEN."

set -a
# shellcheck disable=SC1090
source "${ENV_FILE}"
set +a

: "${SONAR_HOST_URL:?SONAR_HOST_URL must be set in .env}"
: "${SONAR_TOKEN:?SONAR_TOKEN must be set in .env}"

export PATH="${HOME}/.dotnet:${HOME}/.dotnet/tools:${PATH}"
command -v dotnet >/dev/null 2>&1 || fail ".NET SDK not found. Run 'make setup'."
command -v dotnet-sonarscanner >/dev/null 2>&1 || fail "SonarScanner not found. Run 'make setup'."

cd "${REPO_ROOT}"

mkdir -p "${RESULTS_DIRECTORY}"
find "${RESULTS_DIRECTORY}" -mindepth 1 -delete

printf '[sonar] Starting analysis for %s\n' "${PROJECT_KEY}"
dotnet-sonarscanner begin \
    "/k:${PROJECT_KEY}" \
    "/d:sonar.host.url=${SONAR_HOST_URL}" \
    "/d:sonar.token=${SONAR_TOKEN}" \
    "/d:sonar.exclusions=src/**/Program.cs,src/**/Extensions/**/*.cs" \
    "/d:sonar.cs.opencover.reportsPaths=${RESULTS_DIRECTORY}/**/coverage.opencover.xml" \
    "/d:sonar.cs.vstest.reportsPaths=${RESULTS_DIRECTORY}/*.trx"

dotnet restore "${SOLUTION}"
dotnet build "${SOLUTION}" --configuration Release --no-restore
dotnet test "${TEST_PROJECT}" \
    --configuration Release \
    --no-build \
    --logger "trx;LogFileName=tests.trx" \
    --results-directory "${RESULTS_DIRECTORY}" \
    --collect "XPlat Code Coverage" \
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

dotnet-sonarscanner end "/d:sonar.token=${SONAR_TOKEN}"
printf '[sonar] Analysis submitted successfully\n'
