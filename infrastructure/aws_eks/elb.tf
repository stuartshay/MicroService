resource "aws_security_group" "elb" {
  name        = "${var.cluster-name}-eks-worker-node-ELB"
  description = "Security group for ELB"
  vpc_id      = "${data.terraform_remote_state.infra.vpc_id}"

  ingress {
    from_port   = 5000
    to_port     = 5000
    protocol    = "TCP"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 9000
    to_port     = 9000
    protocol    = "TCP"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 9090
    to_port     = 9090
    protocol    = "TCP"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 8025
    to_port     = 8025
    protocol    = "TCP"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = "${
    map(
     "Name", "terraform-eks-worker-node-ELB",
     "kubernetes.io/cluster/${var.cluster-name}", "owned",
    )
  }"
}

resource "aws_elb" "elb" {
  name               = "${var.cluster-name}-k8s-worker-elb"
  subnets            = ["${data.terraform_remote_state.infra.public_subnets}"]
  security_groups    = ["${aws_security_group.elb.id}"]

  listener {
    instance_port     = 30999
    instance_protocol = "tcp"
    lb_port           = 9000
    lb_protocol       = "tcp"
  }

  listener {
    instance_port     = 30925
    instance_protocol = "tcp"
    lb_port           = 8025
    lb_protocol       = "tcp"
  }

  listener {
    instance_port     = 30950
    instance_protocol = "tcp"
    lb_port           = 5000
    lb_protocol       = "tcp"
  }

  listener {
    instance_port     = 30990
    instance_protocol = "tcp"
    lb_port           = 9090
    lb_protocol       = "tcp"
  }

  health_check {
    healthy_threshold   = "${var.healthy_threshold}"
    unhealthy_threshold = "${var.unhealthy_threshold}"
    timeout             = "${var.health_check_timeout}"
    target              = "${var.health_check_target}"
    interval            = "${var.health_check_interval}"
  }

  cross_zone_load_balancing   = "${var.enable_cross_zone_load_balancing}"
  idle_timeout                = "${var.idle_timeout}"
  connection_draining         = "${var.enable_connection_draining}"
  connection_draining_timeout = "${var.connection_draining_timeout}"

}
