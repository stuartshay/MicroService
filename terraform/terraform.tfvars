
# AWS Credentials
region = "us-east-2"

# The name of the cluster (only alphanumeric characters and hyphens allowed)
cluster_name = "coredata-test"

# AWS ECS Task Definition values
# A unique name for your task definition
family_name = "coredata"

# Container values

# Name of container
container_name = "coredatastore"

# Docker image
docker_image = "stuartshay/microservice-api:2.2.1-build"

# The number of conatiners of the task definition to place and keep running
desired_count = "1"

# Fargate instance CPU units to provision
cpu_value = "256"
# Fargate instance memory to provision (in MiB)
memory_value = "512"

# Container port use for mapping with host
container_port = "5000"

# Host port use for mapping with container
host_port = "5000"

########## for AWS ALB ######

# The CIDR block for the VPC
cidr_block = "172.17.0.0/16"

# Number of AZs to cover in a given AWS region (minimum required 2)
az_count = "2"

# AWS ALB values
# The port on which targets receive traffic and on which the load balancer is listening
alb_port = "5000"

# The protocol to use for routing traffic to the targets and for connections from clients to the load balancer
alb_protocol = "HTTP"

# The type of target, possible values are instance or ip
alb_target_type = "ip"


####### for logs ########

# Prefix to use with logs stream
logs_stream_prefix = "ecs"

# The number of days you want to retain log events in the specified log group
retention_days = "7"
