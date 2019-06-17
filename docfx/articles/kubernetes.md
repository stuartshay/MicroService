# Kubernetes

## Prerequisites

- Hyper-V     
[Hyper-V Install](hyper-v.md)
- Powershell  > 6.2   
https://github.com/PowerShell/PowerShell

- Chocolatey   
https://chocolatey.org/docs/installation

- Choco Essentials
```
choco install vscode
choco install googlechrome
choco install openssh
```

## Install Docker Desktop for Windows

```
choco install docker-desktop
choco install kubernetes-helm
```

![](images/docker-desktop.png)


#### Verify Instalation 

```
kubectl version

kubectl config current-context

kubectl cluster-info

kubectl get nodes
```

### Installating the Kubernetes Dashboard

```
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v1.10.1/src/deploy/recommended/kubernetes-dashboard.yaml
```

#### Enable Dashboard & Create Access Token

```
kubectl proxy
```

```
$TOKEN=((kubectl -n kube-system describe secret default | Select-String "token:") -split " +")[1]
kubectl config set-credentials docker-for-desktop --token="${TOKEN}"
```

#### Access Dashboard

```
http://localhost:8001/api/v1/namespaces/kube-system/services/https:kubernetes-dashboard:/proxy/ 
```

```
Click on Kubeconfig and select the “config” file under C:\Users<Username>.kube\config
```

![](images/kube-dashboard.png)






### References 

```
http://collabnix.com/kubernetes-dashboard-on-docker-desktop-for-windows-2-0-0-3-in-2-minutes/
```

*************************************************************************************





# BELOW ARE NOTES CAN BE REMOVED


## Start & Run 




```
minikube start --vm-driver hyperv --hyperv-virtual-switch "Primary Virtual Switch"

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

## References 

```
https://rominirani.com/tutorial-getting-started-with-kubernetes-with-docker-on-mac-7f58467203fd
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

