# Terraform configuration for AWS Postgres RDS
This terraform configuration do the following:
* An AWS Postgres RDS instance.
* A Postgres database.
* A database user with superuser privlidges.
* A table in the database.
* Import data from "test_data_201904121704.csv" file to database table. 

## Prerequisites
* PostgreSQL client
* Terraform
* An AWS account


## Insert Variable values
Enter all the required values in ``` terraform.vars ```


## Export AWS credentials
Provide aws credentials via environment variables

```export AWS_ACCESS_KEY_ID=${aws_access_id}```

```export AWS_SECRET_ACCESS_KEY=${aws_secret_key}```


## Run terraform init
To initialize working directory containing Terraform configuration files

```$ terraform init```

## Run terraform plan
To create an execution plan

```$ terraform plan```

## Run terraform apply
To launch execution plan

```$ terraform apply```

## Run terraform destroy
To destroy the Terraform-managed infrastructure

```$ terraform destroy```
