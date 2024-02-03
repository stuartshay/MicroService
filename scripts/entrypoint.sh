#!/bin/bash

# Example of custom startup logic
echo "Starting MicroService.WebApi..."


# Mount the GCS bucket
#gcsfuse -o nonempty -o allow_other $GCS_BUCKET_NAME /mnt/gcs-bucket

# Start the .NET application
dotnet /app/MicroService.WebApi.dll
