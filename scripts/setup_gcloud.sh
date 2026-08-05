#!/usr/bin/env bash

set -Eeuo pipefail

readonly GCLOUD_INSTALL_DIR="${HOME}/google-cloud-sdk"
readonly GCLOUD_INSTALLER_URL="https://dl.google.com/dl/cloudsdk/channels/rapid/downloads/google-cloud-cli-linux-x86_64.tar.gz"
readonly GCLOUD_INSTALLER_URL_ARM="https://dl.google.com/dl/cloudsdk/channels/rapid/downloads/google-cloud-cli-linux-arm.tar.gz"
readonly GCLOUD_INSTALLER_URL_MACOS_X86="https://dl.google.com/dl/cloudsdk/channels/rapid/downloads/google-cloud-cli-darwin-x86_64.tar.gz"
readonly GCLOUD_INSTALLER_URL_MACOS_ARM="https://dl.google.com/dl/cloudsdk/channels/rapid/downloads/google-cloud-cli-darwin-arm.tar.gz"

log() {
    printf '[setup-gcloud] %s\n' "$*"
}

fail() {
    printf '[setup-gcloud] ERROR: %s\n' "$*" >&2
    exit 1
}

configure_gcloud_path() {
    local profile_file
    local marker='# MicroService Google Cloud SDK'

    export PATH="${GCLOUD_INSTALL_DIR}/bin:${PATH}"

    case "${SHELL:-}" in
        */zsh) profile_file="${HOME}/.zshrc" ;;
        *) profile_file="${HOME}/.bashrc" ;;
    esac

    touch "${profile_file}"
    if ! grep -Fq "${marker}" "${profile_file}"; then
        log "Adding Google Cloud SDK to PATH in ${profile_file}"
        {
            printf '\n%s\n' "${marker}"
            # shellcheck disable=SC2016
            printf 'export PATH="$HOME/google-cloud-sdk/bin:$PATH"\n'
        } >> "${profile_file}"
    fi
}

installer_url() {
    local os arch

    os="$(uname -s)"
    arch="$(uname -m)"

    case "${os}" in
        Linux)
            case "${arch}" in
                x86_64) printf '%s\n' "${GCLOUD_INSTALLER_URL}" ;;
                aarch64|arm64) printf '%s\n' "${GCLOUD_INSTALLER_URL_ARM}" ;;
                *) fail "Unsupported Linux architecture: ${arch}" ;;
            esac
            ;;
        Darwin)
            case "${arch}" in
                x86_64) printf '%s\n' "${GCLOUD_INSTALLER_URL_MACOS_X86}" ;;
                arm64) printf '%s\n' "${GCLOUD_INSTALLER_URL_MACOS_ARM}" ;;
                *) fail "Unsupported macOS architecture: ${arch}" ;;
            esac
            ;;
        *) fail "Unsupported operating system: ${os}" ;;
    esac
}

install_gcloud_cli() {
    local temp_dir archive url

    if command -v gcloud >/dev/null 2>&1; then
        log "Google Cloud CLI already installed: $(gcloud --version | head -n 1)"
        return
    fi

    if [[ -x "${GCLOUD_INSTALL_DIR}/bin/gcloud" ]]; then
        log "Google Cloud CLI found in ${GCLOUD_INSTALL_DIR}"
        return
    fi

    url="$(installer_url)"
    temp_dir="$(mktemp -d)"
    archive="${temp_dir}/google-cloud-cli.tar.gz"

    log "Downloading Google Cloud CLI from ${url}"
    curl --fail --location --retry 3 --silent --show-error "${url}" --output "${archive}"

    log "Installing Google Cloud CLI into ${GCLOUD_INSTALL_DIR}"
    mkdir -p "${HOME}"
    tar -xzf "${archive}" -C "${HOME}"
    rm -rf -- "${temp_dir}"

    "${GCLOUD_INSTALL_DIR}/install.sh" --quiet --usage-reporting=false --path-update=false --command-completion=false
}

verify_gcloud() {
    command -v gcloud >/dev/null 2>&1 || fail "gcloud command not found after installation"
    log "Google Cloud CLI: $(gcloud --version | head -n 1)"

    if gcloud auth list --format="value(account)" 2>/dev/null | grep -q .; then
        log "Authenticated account: $(gcloud config get-value account 2>/dev/null)"
    else
        log "No authenticated account found. Run 'gcloud auth login' to authenticate."
    fi
}

main() {
    install_gcloud_cli
    configure_gcloud_path
    verify_gcloud

    log "Setup complete. Open a new terminal, or run 'source ~/.bashrc' (or ~/.zshrc), then use 'gcloud'."
}

main "$@"
