# PROVIDER VARIABLES
# AWS region
variable "region" {
  default = "us-east-1"
  type        = "string"
}

# Storage allocated to database instance
variable "allocated_storage" {
  description = "Storage allocated to database instance "
  type = "string"
  default = "20"
}

# Database engine version
variable "engine_version" {
  description = "Database engine version"
  type = "string"
  default = "10.7"
}

# Instance type for database instance
variable "instance_type" {
  description = "Instance type for database instance "
  type = "string"
  default = "db.t2.micro"
}

# Type of underlying storage for database
variable "storage_type" {
  description = "Type of underlying storage for database"
  type = "string"
  default = "gp2"
}

# Identifier for RDS instance
variable "database_identifier" {
  description = " Identifier for RDS instance"
  type = "string"
}

# Database password inside storage engine
variable "database_password" {
  description = "Database password inside storage engine"
  type = "string"
}

# Name of user inside storage engine
variable "database_username" {
  description = "Name of user inside storage engine"
  type = "string"
}

# Name of database inside storage engine
variable "database_name" {
  description = "Name of database inside storage engine"
  type = "string"
}

# Port on which database will accept connections
variable "database_port" {
  description = "Port on which database will accept connections"
  type = "string"
  default = "5432"
}

# Number of days to keep database backups
variable "backup_retention_period" {
  description = "Number of days to keep database backups"
  type = "string"
  default = "30"
}

# Identifier for final snapshot if skip_final_snapshot is set to false
variable "final_snapshot_identifier" {
  description = "Identifier for final snapshot if skip_final_snapshot is set to false"
  type = "string"
  default = "terraform-aws-postgresql-rds-snapshot"
}

# Flag to enable or disable a snapshot if the database instance is terminated
variable "skip_final_snapshot" {
  description = "Flag to enable or disable a snapshot if the database instance is terminated"
  type = "string"
  default = true
}

# Flag to enable or disable copying instance tags to the final snapshot
variable "copy_tags_to_snapshot" {
  description = "Flag to enable or disable copying instance tags to the final snapshot"
  type = "string"
  default = false
}

# Flag to enable hot standby in another availability zone
variable "multi_availability_zone" {
  description = "Flag to enable hot standby in another availability zone"
  type = "string"
  default = false
}

# Flag to enable storage encryption
variable "storage_encrypted" {
  description = "Flag to enable storage encryption"
  type = "string"
  default = false
}

# RDS TAGS VARIABLES
# Name of project
variable "project" {
  description = "Name of projectt "
  type = "string"
  default = "Unknown"
}

# Name of environment
variable "environment" {
  description = "Name of environment"
  type = "string"
  default = "Unknown"
}
