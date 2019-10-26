# Terraform configuration for VPC infrastructure
This terraform configuration do the following:
* VPC
* Public Subnet
* Private Subnet
* NAT Gateway
* Elastic IP
* Route tables

## Prerequisites
* Terraform
* An AWS account

## Manage s3 backend for tfstate file
This terraform configuration use Amazon S3 backend to store the terraform state files in bucket `tf-state-fargate` and the terraform state is written to the key `vpc-state.tfstate` in `us-east-2` region.

If you need to change S3 backend values, you can change following properties in `backend.tf` file.
```
terraform {
  backend "s3" {
    bucket = 	"tf-state-fargate"
    key    = 	"vpc-state.tfstate"
    region = 	"us-east-2"
  }
}
```
## Insert Variable values
Enter all the required values in ``` terraform.vars ```


## Export AWS credentials
Provide aws credentials via environment variables

```export AWS_ACCESS_KEY_ID=${aws_access_id}```

```export AWS_SECRET_ACCESS_KEY=${aws_secret_key}```


## Run terraform init
To initialize working directory containing Terraform configuration files

```$ terraform init```
```
Initializing the backend...

Successfully configured the backend "s3"! Terraform will automatically
use this backend unless the backend configuration changes.
```

## Run terraform plan
To create an execution plan

```$ terraform plan```

## Run terraform apply
To launch execution plan

```$ terraform apply```
```
Plan: 15 to add, 0 to change, 0 to destroy.

Do you want to perform these actions?
  Terraform will perform the actions described above.
  Only 'yes' will be accepted to approve.

  Enter a value:
```
If you want to apply the plan enter ```yes``` or enter any other key to abort.
## Run terraform destroy
To destroy the Terraform-managed infrastructure

```$ terraform destroy```
