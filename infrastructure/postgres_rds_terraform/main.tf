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


resource "null_resource" "create_table" {
  depends_on = ["aws_db_instance.postgresql"] #wait for the db to be ready
  provisioner "local-exec" {
    command = "PGPASSWORD=${var.database_password} psql -h ${aws_db_instance.postgresql.address} -U ${var.database_username} -d ${var.database_name} -p ${var.database_port} -c 'CREATE TABLE public.PLUTO (dummy INTEGER NOT NULL PRIMARY KEY)'"
  }
}

# resource "null_resource" "setup_db" {
#   depends_on = ["null_resource.create_table"]
#   provisioner "local-exec" {
#       command = "docker run -t -v $(pwd):/tmp/ node:alpine /bin/sh -c 'npm i map-pluto-postgres && ls -l /tmp && node /tmp/npm_data/pluto.js --host ${aws_db_instance.postgresql.address} --port ${var.database_port} --user ${aws_db_instance.postgresql.username} --password ${var.database_password}  --database ${var.database_name}'"

#   }
# }
