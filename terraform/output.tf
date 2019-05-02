output "alb_hostname" {
value = " You can access the swagger at ${aws_alb.main.dns_name}:${var.alb_port}/swagger"
}
