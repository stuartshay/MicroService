#!/bin/bash

# Mount the GCS bucket
gcsfuse -o nonempty -o allow_other $GCS_BUCKET_NAME /mnt/gcs-bucket

# Start the .NET application
exec dotnet MicroService.WebApi.dll
