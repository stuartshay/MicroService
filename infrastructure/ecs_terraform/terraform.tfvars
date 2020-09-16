
# AWS Credentials
region = "us-east-2"

# The name of the cluster (only alphanumeric characters and hyphens allowed)
cluster_name = "microservice"

# AWS ECS Task Definition values
# A unique name for your task definition
family_name = "microservices"

# Container values

# Name of container
container_name = "microservice"

# # Docker image
#docker_image = ""

# The number of conatiners of the task definition to place and keep running
desired_count = "1"

aspnetcore_envirnoment = "Docker" # api env

db_connection_string = "User ID=nyclandmarks;Password=nyclandmarks;Server=myrdsinstance.ckm3eyorqjco.us-east-1.rds.amazonaws.com;Port=5432;Database=nyclandmarks;Integrated Security=true;Pooling=true;"

# Fargate instance CPU units to provision
cpu_value = "1024"
# Fargate instance memory to provision (in MiB)
memory_value = "2048"

# Container port use for mapping with host
container_port = "5000"

# Host port use for mapping with container
host_port = "5000"

########## for AWS ALB ######

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

# Name of env
env_name = "master"