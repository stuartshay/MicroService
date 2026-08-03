#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
readonly SCRIPT_DIR
REPO_ROOT="$(cd -- "${SCRIPT_DIR}/.." && pwd)"
readonly REPO_ROOT
readonly CERTIFICATE_DIRECTORY="${REPO_ROOT}/docker/nginx/ssl"
readonly CERTIFICATE_FILE="${CERTIFICATE_DIRECTORY}/localhost.crt"
readonly KEY_FILE="${CERTIFICATE_DIRECTORY}/localhost.key"

command -v openssl >/dev/null 2>&1 || {
    printf '[certificates] ERROR: OpenSSL is required. Install OpenSSL, or run make setup on Linux.\n' >&2
    exit 1
}

umask 077
mkdir -p "${CERTIFICATE_DIRECTORY}"

if [[ -s "${CERTIFICATE_FILE}" && -s "${KEY_FILE}" ]]; then
    printf '[certificates] Local development certificate already exists.\n'
    exit 0
fi

if [[ -e "${CERTIFICATE_FILE}" || -e "${KEY_FILE}" ]]; then
    printf '[certificates] ERROR: Incomplete certificate pair. Remove both %s and %s, then retry.\n' \
        "${CERTIFICATE_FILE}" "${KEY_FILE}" >&2
    exit 1
fi

openssl req \
    -x509 \
    -newkey rsa:2048 \
    -sha256 \
    -days 365 \
    -nodes \
    -keyout "${KEY_FILE}" \
    -out "${CERTIFICATE_FILE}" \
    -subj '/CN=localhost' \
    -addext 'subjectAltName=DNS:localhost,IP:127.0.0.1'

chmod 600 "${KEY_FILE}"
printf '[certificates] Generated %s and %s.\n' "${CERTIFICATE_FILE}" "${KEY_FILE}"
