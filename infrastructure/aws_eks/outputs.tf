#
# Outputs
#

locals {
  config_map_aws_auth = <<CONFIGMAPAWSAUTH
apiVersion: v1
kind: ConfigMap
metadata:
  name: aws-auth
  namespace: kube-system
data:
  mapRoles: |
    - rolearn: ${aws_iam_role.worker-node.arn}
      username: system:node:{{EC2PrivateDNSName}}
      groups:
        - system:bootstrappers
        - system:nodes
CONFIGMAPAWSAUTH

  kubeconfig = <<KUBECONFIG
apiVersion: v1
clusters:
- cluster:
    server: ${aws_eks_cluster.eks_cluster.endpoint}
    certificate-authority-data: ${aws_eks_cluster.eks_cluster.certificate_authority.0.data}
  name: kubernetes
contexts:
- context:
    cluster: kubernetes
    user: aws
  name: aws
current-context: aws
kind: Config
preferences: {}
users:
- name: aws
  user:
    exec:
      apiVersion: client.authentication.k8s.io/v1alpha1
      command: aws-iam-authenticator
      args:
        - "token"
        - "-i"
        - "${var.cluster-name}"
KUBECONFIG
}

resource "null_resource" "create_config_file" {
  depends_on = ["aws_eks_cluster.eks_cluster"]
  triggers {
      build_number = "${timestamp()}"
  }
  provisioner "local-exec" {
    command = <<EOT
      curl -o aws-iam-authenticator https://amazon-eks.s3-us-west-2.amazonaws.com/1.13.8/2019-08-14/bin/linux/amd64/aws-iam-authenticator
      chmod +x ./aws-iam-authenticator
      mkdir -p $HOME/bin && cp ./aws-iam-authenticator $HOME/bin/aws-iam-authenticator && export PATH=$HOME/bin:$PATH
      echo 'export PATH=$HOME/bin:$PATH' >> ~/.bashrc
      echo "${local.config_map_aws_auth}" > $HOME/aws-auth-cm.yaml
      mkdir -p $HOME/.kube
      echo "${local.kubeconfig}" > $HOME/.kube/config
      kubectl apply -f $HOME/aws-auth-cm.yaml
      curl https://raw.githubusercontent.com/aws-samples/amazon-cloudwatch-container-insights/master/k8s-yaml-templates/quickstart/cwagent-fluentd-quickstart.yaml | sed "s/{{cluster_name}}/${var.cluster-name}/;s/{{region_name}}/${var.region}/" | kubectl apply -f -
      kubectl get nodes -o jsonpath="{.items[0].status.addresses[1].address}" > $HOME/.nodeip
      aws s3 mv $HOME/.kube/config s3://${var.eks_state_bucket_name}/${var.branch_name}-admin.config
      aws s3 mv $HOME/aws-auth-cm.yaml s3://${var.eks_state_bucket_name}/${var.branch_name}-aws-auth-cm.yaml
EOT
  }
}
