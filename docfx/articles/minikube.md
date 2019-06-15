# Details 2

## Install minikube for Windows (Vitural Box)

```
choco install minikube

choco install kubernetes-cli
```

## Helm

```
kubectl create serviceaccount --namespace kube-system tiller

kubectl create clusterrolebinding tiller-cluster-rule --clusterrole=cluster-admin --serviceaccount=kube-system:tiller
```

