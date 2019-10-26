# Terraform VPC state file
data "terraform_remote_state" "infrastructure" {
  backend = "s3"
  config {
    bucket = "tf-state-fargate"
    key    = "vpc-fargate-state.tfstate"
    region = "us-east-2"
  }
}
