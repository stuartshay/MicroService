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

# ELB Health Check
# The number of checks before the instance is declared healthy
variable "healthy_threshold" {
  description = "checks count before the instance is declared healthy"
  type        = "string"
}

# The number of checks before the instance is declared unhealthy
variable "unhealthy_threshold" {
  description = "checks count before the instance is declared unhealthy"
  type        = "string"
}

# The length of time before the check times out
variable "health_check_timeout" {
  description = "length of time before the check times out"
  type        = "string"
}

# The target of the check. Valid pattern is "${PROTOCOL}:${PORT}${PATH} (eg. HTTP:8080/)"
variable "health_check_target" {
  description = "target of the check"
  type        = "string"
}

# The interval between checks
variable "health_check_interval" {
  description = "interval between checks"
  type        = "string"
}

# AWS ELB variables
# Enable cross-zone load balancing.
variable "enable_cross_zone_load_balancing" {
  description = "Enable cross-zone load balancing"
  default     = "true"
  type        = "string"
}

# The time in seconds that the connection is allowed to be idle
variable "idle_timeout" {
  description = "time in seconds that the connection is allowed to be idle"
  default = "60"
}

# Boolean to enable connection draining
variable "enable_connection_draining" {
  description = "Boolean to enable connection draining"
  default     = "false"
  type        = "string"
}

# The time in seconds to allow for connections to drain
variable "connection_draining_timeout" {
  description = "time in seconds to allow for connections to drain"
  default     = "300"
  type        = "string"
}
