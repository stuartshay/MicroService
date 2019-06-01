resource "aws_alb" "main" {
  name            = "${var.cluster_name}-alb"
  subnets         = ["${aws_subnet.public.*.id}"]
  security_groups = ["${aws_security_group.lb.id}"]
}

resource "aws_alb_target_group" "app" {
  name        = "${var.cluster_name}-alb-targer-group"
  port        = "${var.alb_port}"
  protocol    = "${var.alb_protocol}"
  vpc_id      = "${aws_vpc.main.id}"
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
