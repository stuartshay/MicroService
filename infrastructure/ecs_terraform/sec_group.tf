# ALB Security group

resource "aws_security_group" "lb" {
  name        = "${var.cluster_name}-alb-sec-group"
  description = "controls access to the ALB"
  vpc_id      = "${data.terraform_remote_state.infrastructure.vpc_id}"

  ingress {
    from_port   = "${var.alb_port}"
    to_port     = "${var.alb_port}"
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port = 0
    to_port   = 0
    protocol  = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# Traffic to the ECS Cluster should only come from the ALB

resource "aws_security_group" "ecs_tasks" {
  name        = "${var.cluster_name}-sec-group"
  description = "allow inbound access from the ALB only"
  vpc_id      = "${data.terraform_remote_state.infrastructure.vpc_id}"

  ingress {
    protocol        = "tcp"
    from_port       = "${var.container_port}"
    to_port         = "${var.container_port}"
    security_groups = ["${aws_security_group.lb.id}"]
  }

  egress {
    protocol    = "-1"
    from_port   = 0
    to_port     = 0
    cidr_blocks = ["0.0.0.0/0"]
  }
}
