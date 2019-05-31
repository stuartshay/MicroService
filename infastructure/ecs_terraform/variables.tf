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

# The name of the cluster
variable "cluster_name" {
  description = "only alphanumeric characters and hyphens allowed"
  type        = "string"
  }

# AWS ECS Task Definition values
# Task definition family name
variable "family_name" {
  description = "A unique name for your task definition"
  type        = "string"
  }

# CPU value at container level for fargate
variable "cpu_value" {
  description = "Fargate instance CPU units to provision (1 vCPU = 1024 CPU units)"
  type        = "string"
  }

# Memory value at containerlevel for fargate
variable "memory_value" {
  description = "Fargate instance memory to provision (in MiB)"
  type        = "string"
  }

# The number of instances of the task definition to place and keep running
  variable "desired_count" {
    description = "Number of minimum docker containers to run"
    type        = "string"
  }

# Docker image to run in the ECS cluster
  variable "docker_image" {
    description = "Docker image to run in the ECS cluster"
    type        = "string"
  }

# Container port use for mapping with host
  variable "container_port" {
  description = "Container port number to map with host"
  type        = "string"
}

# Host port use for mapping with container
  variable "host_port" {
  description = "Host port number to map with container"
  type        = "string"
}

# The name of a container
variable "container_name" {
description = "Name of container"
type        = "string"
}

# AWS ALB values
# The port on which targets receive traffic and on which the load balancer is listening
variable "alb_port" {
description = "port number to use with alb"
type        = "string"
}

# The protocol to use for routing traffic to the targets and for connections from clients to the load balancer
variable "alb_protocol" {
description = "protocol use with alb"
type        = "string"
}

# The type of target, possible values are instance or ip
variable "alb_target_type" {
description = "ALB target type"
type        = "string"
}

# AWS logs stream prefix
variable "logs_stream_prefix" {
description = "prefix to use with logs stream"
type        = "string"
}

# The number of days you want to retain log events in the specified log group
variable "retention_days" {
description = "number of days you want to retain log events"
type        = "string"
}
variable "db_connection_string" {
description = "number of days you want to retain log events"
type        = "string"
}

variable "aspnetcore_envirnoment" {
description = "number of days you want to retain log events"
type        = "string"
}
