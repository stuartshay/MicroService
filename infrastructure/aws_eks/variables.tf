#
# Variables Configuration
# Name of AWS EKS cluster
variable "region" {
  description = "aws region"
  type        = "string"
}

# Terraform VPC state file bucket name
variable "vpc_state_bucket_name" {
  description = "terraform VPC state file bucket name"
}

# Terraform EKS state file bucket name
variable "eks_state_bucket_name" {
  description = "terraform EKS state file bucket name"
}

#Git Branch Name on runtime
variable "branch_name" {
  description = "Git Branch Name on runtime"
}

# Name of AWS EKS cluster
variable "cluster-name" {
  default = "terraform-eks"
  type    = "string"
}

# The size of worker node to launch
variable "instance_type" {
  description = "AWS instance type"
}

# Maximum number of worker nodes to be started
variable "max_instances" {
  description = " Max Number of instances"
  default     = "1"
  type        = "string"
}

# Minimum number of worker nodes to be started
variable "min_instances" {
  description = "Min Number of instances"
  default     = "1"
  type        = "string"
}
