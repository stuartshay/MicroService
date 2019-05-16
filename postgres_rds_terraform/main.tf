resource "aws_db_instance" "postgresql" {
  allocated_storage               = "${var.allocated_storage}"
  engine                          = "postgres"
  engine_version                  = "${var.engine_version}"
  identifier                      = "${var.database_identifier}"
  instance_class                  = "${var.instance_type}"
  storage_type                    = "${var.storage_type}"
  name                            = "${var.database_name}"
  username                        = "${var.database_username}"
  password                        = "${var.database_password}"
  publicly_accessible             = true
  backup_retention_period         = "${var.backup_retention_period}"
  enabled_cloudwatch_logs_exports = ["postgresql","upgrade"]
  final_snapshot_identifier       = "${var.final_snapshot_identifier}"
  skip_final_snapshot             = "${var.skip_final_snapshot}"
  copy_tags_to_snapshot           = "${var.copy_tags_to_snapshot}"
  multi_az                        = "${var.multi_availability_zone}"
  port                            = "${var.database_port}"
  vpc_security_group_ids          = ["${aws_security_group.postgresql.id}"]
  storage_encrypted               = "${var.storage_encrypted}"

  tags {
    Name        = "${var.database_name}"
    Project     = "${var.project}"
    Environment = "${var.environment}"
  }
}


resource "null_resource" "setup_db" {
  depends_on = ["aws_db_instance.postgresql"] #wait for the db to be ready
  provisioner "local-exec" {
    command = "psql -h ${aws_db_instance.postgresql.address} -U ${aws_db_instance.postgresql.username} ${var.database_name} -p ${var.database_port} -W ${var.database_password} -a -q -f ${path.module}/sql_scripts/init1.sql"
  }
}
