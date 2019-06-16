# Minikube

## Prerequisites

VirtualBox > 6.0.8   

```
https://www.virtualbox.org/
```

Install Powershell > 6.2

```
https://github.com/PowerShell/PowerShell
```
- Chocolatey   
https://chocolatey.org/docs/installation

Verify Version 
```
$PSVersionTable.PSVersion

Major  Minor  Patch  PreReleaseLabel BuildLabel
-----  -----  -----  --------------- ----------
6      2      1
```

## Install minikube for Windows (Vitural Box)

```
choco install minikube
choco install kubernetes-cli
choco install kubernetes-helm
```

## Start & Run 

```
minikube start
```


### Verify Install

```
minikube status

host: Running
kubelet: Running
apiserver: Running
kubectl: Correctly Configured: pointing to minikube-vm at 192.168.99.100
```

```
kubectl get nodes
```


### Create Service Account

https://github.com/kubernetes/dashboard/wiki/Creating-sample-user



Powershell script to automate generation of kubeconfig for the Kubernetes use      
https://community.pivotal.io/s/article/powershell-script-to-automate-generation-of-kubeconfig-for-the-kubernetes-use


### Deploying the Dashboard UI

https://kubernetes.io/docs/tasks/access-application-cluster/web-ui-dashboard/

```
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/master/aio/deploy/recommended/kubernetes-dashboard.yaml
```











## Helm and Tiller

```
kubectl config view
```

```
apiVersion: v1
clusters:
- cluster:
    certificate-authority: C:\Users\stuar\.minikube\ca.crt
    server: https://192.168.99.100:8443
  name: minikube
contexts:
- context:
    cluster: minikube
    user: minikube
  name: minikube
current-context: minikube
kind: Config
preferences: {}
users:
- name: minikube
  user:
    client-certificate: C:\Users\stuar\.minikube\client.crt
    client-key: C:\Users\stuar\.minikube\client.key
```

## Starting Helm

```
helm init

kubectl describe deploy tiller-deploy --namespace=kube-system
```

[!NOTE] This is a note which needs your attention, but it's not super important.


















```
kubectl create serviceaccount --namespace kube-system tiller

kubectl create clusterrolebinding tiller-cluster-rule --clusterrole=cluster-admin --serviceaccount=kube-system:tiller
```

