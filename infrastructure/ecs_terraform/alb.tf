resource "aws_alb" "main" {
  name            = "${var.cluster_name}-alb"
  subnets         = ["${data.terraform_remote_state.infrastructure.public_subnets}"]
  security_groups = ["${aws_security_group.lb.id}"]
  provisioner "local-exec" {
    command = "echo ${aws_alb.main.dns_name}:${var.alb_port}/swagger > .endpoint"
  }
}

resource "aws_alb_target_group" "app" {
  name        = "${var.cluster_name}-alb-targer-group"
  port        = "${var.alb_port}"
  protocol    = "${var.alb_protocol}"
  vpc_id      = "${data.terraform_remote_state.infrastructure.vpc_id}"
  target_type = "${var.alb_target_type}"
}

# Redirect all traffic from the ALB to the target group
resource "aws_alb_listener" "front_end" {
  load_balancer_arn = "${aws_alb.main.id}"
  port              = "${var.alb_port}"
  protocol          = "${var.alb_protocol}"

  default_action {
    target_group_arn = "${aws_alb_target_group.app.id}"
    type             = "forward"
  }
}
