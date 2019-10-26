resource "aws_ecs_task_definition" "app" {
  family                   = "${var.family_name}"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "${var.cpu_value}"
  memory                   = "${var.memory_value}"
  execution_role_arn       = "${aws_iam_role.ecsTaskExecutionRole.arn}"

  container_definitions = <<DEFINITION
[
  {
    "cpu": ${var.cpu_value},
    "image": "${var.docker_image}",
    "memory": ${var.memory_value},
    "name": "${var.container_name}",
    "networkMode": "awsvpc",
    "portMappings": [
      {
        "containerPort": ${var.container_port},
        "hostPort": ${var.host_port}
      }
    ],
    "environment": [
      {
      "name": "ConnectionStrings__PostgreSql",
      "value": "${var.db_connection_string}"
    },
    {
      "name": "ASPNETCORE_ENVIRONMENT",
      "value": "${var.aspnetcore_envirnoment}"
    }
    ],
    "logConfiguration": {
    "logDriver": "awslogs",
    "options": {
      "awslogs-group": "/fargate/service/${var.cluster_name}-${var.container_name}",
      "awslogs-region": "${var.region}",
      "awslogs-stream-prefix": "${var.logs_stream_prefix}"
     }
    }
  }
]
DEFINITION
}

resource "aws_ecs_service" "main" {
  name            = "${var.cluster_name}-fargate-service"
  cluster         = "${aws_ecs_cluster.main.id}"
  task_definition = "${aws_ecs_task_definition.app.arn}"
  desired_count   = "${var.desired_count}"
  launch_type     = "FARGATE"

  network_configuration {
    security_groups = ["${aws_security_group.ecs_tasks.id}"]
    subnets         = ["${data.terraform_remote_state.infrastructure.private_subnets}"]
  }

  load_balancer {
    target_group_arn = "${aws_alb_target_group.app.id}"
    container_name   = "${var.container_name}"
    container_port   = "${var.container_port}"
  }

  depends_on = [
    "aws_alb_listener.front_end",
  ]
}
