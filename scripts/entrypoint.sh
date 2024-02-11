#!/bin/bash

# Example of custom startup logic
echo "Starting MicroService.WebApi..."

export GOOGLE_APPLICATION_CREDENTIALS="/etc/gcsfuse/key/service-account-key.json"

# Check if MOUNT_GCS_BUCKET is set to "true" and the GCS_BUCKET_NAME is not empty
if [[ "${MOUNT_GCS_BUCKET}" == "true" && -n "${GCS_BUCKET_NAME}" ]]; then
    echo "Mounting GCS Bucket: ${GCS_BUCKET_NAME}"
    # Attempt to mount the GCS bucket
    gcsfuse -o nonempty -o allow_other "${GCS_BUCKET_NAME}" /mnt/gcs-bucket
    if [[ $? -ne 0 ]]; then
        echo "Failed to mount GCS bucket. Exiting."
        exit 1
    fi
else
    echo "GCS Bucket mounting skipped."
fi

# Start the .NET application
dotnet /app/MicroService.WebApi.dll
