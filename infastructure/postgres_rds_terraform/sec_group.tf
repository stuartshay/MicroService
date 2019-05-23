resource "aws_security_group" "postgresql" {
  name = "${var.project}-rds-security_group"

  ingress {
      from_port   = "${var.database_port}"
      to_port     = "${var.database_port}"
      protocol    = "tcp"
      cidr_blocks = ["0.0.0.0/0"]
    }

    egress {
      from_port       = 0
      to_port         = 0
      protocol        = "-1"
      cidr_blocks     = ["0.0.0.0/0"]
    }
  }
