
# AWS Credentials
region = "us-east-1"

# RDS Values
# Storage allocated to database instance
allocated_storage = "20"

# Database engine version
engine_version = "10.7"

# Instance type for database instance
instance_type = "db.t2.micro"

# Type of underlying storage for database
storage_type = "gp2"

# Identifier for RDS instance
database_identifier = "myrdsinstance"

# Database password inside storage engine
database_password = "development"

# Name of user inside storage engine
database_username = "development"

# Name of database inside storage engine
database_name = "test"

# Port on which database will accept connections
database_port = "5432"

# Number of days to keep database backups
backup_retention_period = "30"

# Identifier for final snapshot if skip_final_snapshot is set to false
final_snapshot_identifier = "postgres-rds"

# Flag to enable or disable a snapshot if the database instance is terminated
skip_final_snapshot = "true"

# Flag to enable or disable copying instance tags to the final snapshot
copy_tags_to_snapshot = "false"

# Flag to enable hot standby in another availability zone
multi_availability_zone = "false"

# Flag to enable storage encryption
storage_encrypted = "false"

# RDS TAGS VARIABLES
# Name of project
project = "myproject"

# Name of environment
environment = "testing"
