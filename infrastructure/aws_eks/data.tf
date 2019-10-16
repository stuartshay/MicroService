# Get VPC ID
data "terraform_remote_state" "infra" {
  backend = "s3"
  config  = {
    bucket = "${var.vpc_state_bucket_name}"
    key    = "vpc-fargate-state.tfstate"
    region = "${var.region}"
  }
}
