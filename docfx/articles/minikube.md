# Minikube

## Install minikube for Windows (Vitural Box)

```
choco install minikube

choco install kubernetes-cli

choco install kubernetes-helm
```

### Verify Install

```
minikube status
```

```
host: Running
kubelet: Running
apiserver: Running
kubectl: Correctly Configured: pointing to minikube-vm at 192.168.99.100
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




















```
kubectl create serviceaccount --namespace kube-system tiller

kubectl create clusterrolebinding tiller-cluster-rule --clusterrole=cluster-admin --serviceaccount=kube-system:tiller
```

