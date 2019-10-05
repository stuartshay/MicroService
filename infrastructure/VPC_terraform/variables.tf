# PROVIDER VARIABLES
# AWS region
variable "region" {
  default = "us-east-1"
  type        = "string"
}

# The CIDR block for the VPC
variable "cidr_block" {
  description = "CIDR block for the VPC"
  type        = "string"
}

# Number of AZs to cover in a given AWS region
variable "az_count" {
  description = "Number of AZs to cover in a given AWS region"
  type        = "string"
}
