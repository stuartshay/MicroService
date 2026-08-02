#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
readonly SCRIPT_DIR
REPO_ROOT="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly REPO_ROOT
readonly GLOBAL_JSON="${REPO_ROOT}/global.json"
readonly DOTNET_INSTALL_DIR="${HOME}/.dotnet"
readonly DOTNET_INSTALL_URL="https://dot.net/v1/dotnet-install.sh"

log() {
    printf '[setup] %s\n' "$*"
}

fail() {
    printf '[setup] ERROR: %s\n' "$*" >&2
    exit 1
}

read_sdk_version() {
    local version

    [[ -f "${GLOBAL_JSON}" ]] || fail "Missing ${GLOBAL_JSON}"
    version="$(sed -nE 's/^[[:space:]]*"version"[[:space:]]*:[[:space:]]*"([^"]+)".*/\1/p' "${GLOBAL_JSON}" | head -n 1)"
    [[ -n "${version}" ]] || fail "Could not read the SDK version from global.json"

    printf '%s\n' "${version}"
}

run_as_root() {
    if [[ "${EUID}" -eq 0 ]]; then
        "$@"
    elif command -v sudo >/dev/null 2>&1; then
        sudo "$@"
    else
        fail "Root access is required to install operating-system packages"
    fi
}

install_linux_prerequisites() {
    local icu_package

    [[ -r /etc/os-release ]] || fail "Cannot identify this Linux distribution"

    # shellcheck disable=SC1091
    source /etc/os-release
    case "${ID:-}" in
        ubuntu|debian)
            log "Installing prerequisite packages with apt"
            run_as_root apt-get update
            icu_package="$(apt-cache pkgnames | grep -E '^libicu[0-9]+$' | sort -V | tail -n 1 || true)"
            [[ -n "${icu_package}" ]] || fail "Could not find the ICU runtime package"
            run_as_root apt-get install -y --no-install-recommends ca-certificates curl git "${icu_package}" make unzip
            ;;
        *)
            fail "Unsupported Linux distribution: ${ID:-unknown}. Install curl, git, make, and unzip manually."
            ;;
    esac
}

install_macos_prerequisites() {
    if ! xcode-select -p >/dev/null 2>&1; then
        fail "Install the Xcode Command Line Tools with 'xcode-select --install', then rerun this script"
    fi

    for command_name in curl git make unzip; do
        command -v "${command_name}" >/dev/null 2>&1 || fail "Required command not found: ${command_name}"
    done
}

install_prerequisites() {
    case "$(uname -s)" in
        Linux) install_linux_prerequisites ;;
        Darwin) install_macos_prerequisites ;;
        *) fail "Unsupported operating system: $(uname -s)" ;;
    esac
}

configure_dotnet_path() {
    local profile_file
    local marker='# MicroService .NET SDK'

    export DOTNET_ROOT="${DOTNET_INSTALL_DIR}"
    export PATH="${DOTNET_ROOT}:${DOTNET_ROOT}/tools:${PATH}"

    case "${SHELL:-}" in
        */zsh) profile_file="${HOME}/.zshrc" ;;
        *) profile_file="${HOME}/.bashrc" ;;
    esac

    touch "${profile_file}"
    if ! grep -Fq "${marker}" "${profile_file}"; then
        log "Adding .NET to PATH in ${profile_file}"
        {
            printf '\n%s\n' "${marker}"
            # Preserve these variables for expansion when the shell profile is loaded.
            # shellcheck disable=SC2016
            printf 'export DOTNET_ROOT="$HOME/.dotnet"\n'
            # shellcheck disable=SC2016
            printf 'export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"\n'
        } >> "${profile_file}"
    fi
}

install_dotnet_sdk() {
    local sdk_version="$1"
    local temp_dir
    local installer

    configure_dotnet_path

    if command -v dotnet >/dev/null 2>&1 && dotnet --list-sdks | awk '{print $1}' | grep -Fxq "${sdk_version}"; then
        log ".NET SDK ${sdk_version} is already installed"
        return
    fi

    temp_dir="$(mktemp -d)"
    installer="${temp_dir}/dotnet-install.sh"

    log "Installing .NET SDK ${sdk_version} into ${DOTNET_INSTALL_DIR}"
    curl --fail --location --retry 3 --silent --show-error "${DOTNET_INSTALL_URL}" --output "${installer}"
    bash "${installer}" --version "${sdk_version}" --install-dir "${DOTNET_INSTALL_DIR}"
    rm -rf -- "${temp_dir}"
}

verify_toolchain() {
    local expected_sdk="$1"
    local actual_sdk

    actual_sdk="$(cd "${REPO_ROOT}" && dotnet --version)"
    [[ "${actual_sdk}" == "${expected_sdk}" ]] || fail "Expected .NET SDK ${expected_sdk}, found ${actual_sdk}"

    log ".NET SDK: ${actual_sdk}"
    log "Git: $(git --version)"
    log "Make: $(make --version | head -n 1)"

    if command -v docker >/dev/null 2>&1; then
        log "Docker: $(docker --version)"
    else
        log "Docker is optional and was not found; container targets will be unavailable"
    fi
}

repair_generated_artifact_permissions() {
    local artifact_dir
    local -a unwritable_dirs=()

    while IFS= read -r -d '' artifact_dir; do
        if [[ ! -w "${artifact_dir}" ]]; then
            unwritable_dirs+=("${artifact_dir}")
        fi
    done < <(find "${REPO_ROOT}/src" "${REPO_ROOT}/test" \
        -type d \( -name bin -o -name obj \) -prune -print0)

    if (( ${#unwritable_dirs[@]} == 0 )); then
        return
    fi

    log "Repairing permissions on generated build directories"
    for artifact_dir in "${unwritable_dirs[@]}"; do
        run_as_root chown -R "$(id -u):$(id -g)" "${artifact_dir}"
    done
}

main() {
    local sdk_version

    sdk_version="$(read_sdk_version)"
    install_prerequisites
    install_dotnet_sdk "${sdk_version}"
    verify_toolchain "${sdk_version}"
    repair_generated_artifact_permissions

    log "Restoring NuGet packages"
    (cd "${REPO_ROOT}" && dotnet restore MicroService.sln)

    log "Setup complete. Open a new terminal, then run 'make build' or 'make run'."
}

main "$@"
