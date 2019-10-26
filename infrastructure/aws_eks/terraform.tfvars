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

# ELB Health Check
# The number of checks before the instance is declared healthy
healthy_threshold = "2"

# The number of checks before the instance is declared unhealthy
unhealthy_threshold = "5"

# The length of time before the check times out
health_check_timeout = "5"

# The target of the check. Valid pattern is "${PROTOCOL}:${PORT}${PATH} (eg. HTTP:8080/)"
health_check_target = "TCP:30999"

# The interval between checks
health_check_interval = "30"
