# AWS region
region = "us-east-2"

# Terraform VPC state file bucket name
vpc_state_bucket_name = "tf-state-fargate"

# Terraform EKS Cluster state file bucket name
eks_state_bucket_name = "tf-state-eks-cluster"

# EKS Cluster Name
cluster-name = "eks-testing"

# The size of worker node to launch
instance_type = "t2.medium"

# Maximum number of worker nodes to be started
max_instances = "1"

# Minimum number of worker nodes to be started
min_instances = "1"
