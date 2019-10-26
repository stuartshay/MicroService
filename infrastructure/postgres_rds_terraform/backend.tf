terraform {
  backend "s3" {
    bucket = 	"tf-state-fargate"
    key    = 	"rds-state.tfstate"
    region = 	"us-east-2"
  }
}
