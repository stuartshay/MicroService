#!/bin/bash

# Set default values
DEFAULT_SERVICE_NAME="microservice-api"
DEFAULT_BUCKET_NAME="files-shapes"  # Bucket name without the directory
DEFAULT_MOUNT_PATH="/mnt/data/shapes"  # Mount path can remain the same
DEFAULT_REGION="us-east4"  # Changed to your preferred region

# Use command line arguments if provided, otherwise use defaults
SERVICE_NAME=${1:-$DEFAULT_SERVICE_NAME}
BUCKET_NAME=${2:-$DEFAULT_BUCKET_NAME}
MOUNT_PATH=${3:-$DEFAULT_MOUNT_PATH}
REGION=${4:-$DEFAULT_REGION}  # Accept region as an optional fourth argument

# Define the volume name
VOLUME_NAME="files-shapes-volume"

echo "Updating Cloud Run service: $SERVICE_NAME in region: $REGION"
echo "Mounting GCS Bucket: $BUCKET_NAME at $MOUNT_PATH"

# Update the Cloud Run service to mount the GCS bucket
gcloud beta run services update $SERVICE_NAME \
  --execution-environment gen2 \
  --add-volume=name=$VOLUME_NAME,type=cloud-storage,bucket=$BUCKET_NAME \
  --add-volume-mount=volume=$VOLUME_NAME,mount-path=$MOUNT_PATH \
  --region=$REGION  # Specify the region

# Check the exit status of the gcloud command
if [ $? -eq 0 ]; then
  echo "Cloud Run service updated successfully."
else
  echo "Failed to update Cloud Run service."
  exit 1
fi
