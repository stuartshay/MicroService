terraform {
  backend "s3" {
    bucket = 	"tf-state-fargate"
    key    = 	"fargate-state.tfstate"
    region = 	"us-east-2"
  }
}
