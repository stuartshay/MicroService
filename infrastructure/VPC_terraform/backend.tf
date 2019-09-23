terraform {
  backend "s3" {
    bucket = 	"tf-state-fargate"
    key    = 	"vpc-state.tfstate"
    region = 	"us-east-2"
  }
}
