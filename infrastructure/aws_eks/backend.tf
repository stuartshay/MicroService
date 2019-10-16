terraform {
  backend "s3" {
    region = "us-east-2"
    bucket = "tf-state-eks-cluster"
    key = "eks-cluster.tfstate"
  }
}
