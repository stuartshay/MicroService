#
# EKS Worker Nodes Resources
#  * IAM role allowing Kubernetes actions to access other AWS services
#  * EC2 Security Group to allow networking traffic
#  * Data source to fetch latest EKS worker AMI
#  * AutoScaling Launch Configuration to configure worker instances
#  * AutoScaling Group to launch worker instances
#

resource "aws_iam_role" "worker-node" {
  name = "${var.cluster-name}-worker-role"

  assume_role_policy = <<POLICY
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Service": "ec2.amazonaws.com"
      },
      "Action": "sts:AssumeRole"
    }
  ]
}
POLICY
}

resource "aws_iam_role_policy_attachment" "worker-node-AmazonEKSWorkerNodePolicy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKSWorkerNodePolicy"
  role       = "${aws_iam_role.worker-node.name}"
}

resource "aws_iam_role_policy_attachment" "worker-node-AmazonEKS_CNI_Policy" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEKS_CNI_Policy"
  role       = "${aws_iam_role.worker-node.name}"
}

resource "aws_iam_role_policy_attachment" "worker-node-AmazonEC2ContainerRegistryReadOnly" {
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
  role       = "${aws_iam_role.worker-node.name}"
}

resource "aws_iam_role_policy_attachment" "worker-node-cloudwatch" {
  policy_arn = "arn:aws:iam::aws:policy/CloudWatchAgentServerPolicy"
  role       = "${aws_iam_role.worker-node.name}"
}
resource "aws_iam_instance_profile" "eks-instance-profile" {
  name = "${var.cluster-name}-eks-instance-profile"
  role = "${aws_iam_role.worker-node.name}"
}

resource "aws_security_group" "eks-worker" {
  name        = "${var.cluster-name}-eks-worker-node"
  description = "Security group for all nodes in the cluster"
  vpc_id      = "${data.terraform_remote_state.infra.vpc_id}"

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = "${
    map(
     "Name", "terraform-eks-worker-node",
     "kubernetes.io/cluster/${var.cluster-name}", "owned",
    )
  }"
}

resource "aws_security_group_rule" "worker-node-ingress-self" {
  description              = "Allow node to communicate with each other"
  from_port                = 0
  protocol                 = "-1"
  security_group_id        = "${aws_security_group.eks-worker.id}"
  source_security_group_id = "${aws_security_group.eks-worker.id}"
  to_port                  = 65535
  type                     = "ingress"
}

resource "aws_security_group_rule" "worker-node-ingress-elb" {
  description              = "Allow node to communicate with ELB"
  from_port                = 0
  protocol                 = "-1"
  security_group_id        = "${aws_security_group.eks-worker.id}"
  source_security_group_id = "${aws_security_group.elb.id}"
  to_port                  = 65535
  type                     = "ingress"
}

resource "aws_security_group_rule" "worker-node-ingress-cluster" {
  description              = "Allow worker Kubelets and pods to receive communication from the cluster control plane"
  from_port                = 1025
  protocol                 = "tcp"
  security_group_id        = "${aws_security_group.eks-worker.id}"
  source_security_group_id = "${aws_security_group.eks-cluster.id}"
  to_port                  = 65535
  type                     = "ingress"
}

data "aws_ami" "eks-worker" {
  filter {
    name   = "name"
    values = ["amazon-eks-node-${aws_eks_cluster.eks_cluster.version}-v*"]
  }

  most_recent = true
  owners      = ["602401143452"] # Amazon EKS AMI Account ID
}

locals {
  demo-node-userdata = <<USERDATA
#!/bin/bash
set -o xtrace
/etc/eks/bootstrap.sh --apiserver-endpoint '${aws_eks_cluster.eks_cluster.endpoint}' --b64-cluster-ca '${aws_eks_cluster.eks_cluster.certificate_authority.0.data}' '${var.cluster-name}'
USERDATA
}

resource "aws_launch_configuration" "eks_launch_configuration" {
  associate_public_ip_address = true
  iam_instance_profile        = "${aws_iam_instance_profile.eks-instance-profile.name}"
  image_id                    = "${data.aws_ami.eks-worker.id}"
  instance_type               = "${var.instance_type}"
  name_prefix                 = "${var.cluster-name}-eks-worker"
  security_groups             = ["${aws_security_group.eks-worker.id}"]
  user_data_base64            = "${base64encode(local.demo-node-userdata)}"

  lifecycle {
    create_before_destroy = true
  }
}

resource "aws_autoscaling_group" "eks_autoscaling_group" {
  launch_configuration = "${aws_launch_configuration.eks_launch_configuration.id}"
  max_size             = "${var.max_instances}"
  min_size             = "${var.min_instances}"
  name                 = "${var.cluster-name}-eks-asg"
  load_balancers       = ["${aws_elb.elb.name}"]
  vpc_zone_identifier  = ["${data.terraform_remote_state.infra.public_subnets}"]

  tag {
    key                 = "Name"
    value               = "terraform-eks"
    propagate_at_launch = true
  }

  tag {
    key                 = "kubernetes.io/cluster/${var.cluster-name}"
    value               = "owned"
    propagate_at_launch = true
  }
}
