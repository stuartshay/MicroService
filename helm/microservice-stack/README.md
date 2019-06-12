# Helm Chart
## Introduction
This chart deploy all the deployments on a Kubernetes cluster/Minikube using the Helm package manager.


## Application Installed
The following packages will be installed by this helm chart
* Database
* Microservice-api
* Grafana
* Prometheus

## Prerequisite
* Kubernetes or Minikube installed
* Helm

## Installing the Chart

If you need to update the variable values you can change in the values.yaml file in the helm chart

To install the chart run the below command in the home of the  project. make sure you have helm installed in your minikube/kubernetes cluster

```sh
$ helm install --name=name_of_the_release  .
```
Confirm the deployment by, running the command:

```sh
$ helm status name_of_the_release
```

## Uninstalling the Chart

To uninstall/delete the all the deployments

```sh
$ helm del --purge name_of_the_release
```

### To acccess the service 
 
 ```sh
 $ kubectl get svc
 ```
 
```

testing-stuart-microservice-stack-database           NodePort       10.102.17.132    <none>        5432:32459/TCP        137m
testing-stuart-microservice-stack-grafana            NodePort       10.104.227.61    <none>        3000:31188/TCP        137m                                                                                                                                                                                                                  137m
testing-stuart-microservice-stack-microservice-api   NodePort       10.98.20.40      <none>        5000:32583/TCP        137m
testing-stuart-microservice-stack-prometheus         NodePort       10.111.189.141   <none>        9090:30718/TCP        137m    

```
To access microservice-api type NodeIP:port-no/swagger ( e.g nodeip:32583/swagger)

To access grafana type NodeIP:port-no ( e.g nodeip:31188)

To access prometheus type NodeIP:port-no/swagger ( e.g nodeip:30718)

## Configuration

The following table lists the configurable parameters for this helm chart and their default values set.

### Microservice-api
| Parameter	  | Description | Default |
| ------      | ------      | ------ |             
| replicaCount | replica for your deployment | 1 |
| microserviceapi.image.repository| image repository for microserviceapi | `stuartshay/navigator-maps-api:2.2-3-local` |
| microserviceapi.ASPNETCORE_ENVIRONMENT | ASPNETCORE_ENVIRONMENT variable value | Docker |
| microserviceapi.User_ID | user_ID for microserviceapi | postgres |
| microserviceapi.Password| Password microserviceapi | password |
| microserviceapi.Database| Database microserviceapi | postgres |
| microserviceapi.Port|  Port no. for microserviceapi  | `5000` |
| microserviceapi.Integrated_Security | Integrated_Security for microserviceapi | true |
| microserviceapi.Pooling | Pooling for microserviceapi | true |
| microserviceapi.service.type | service type for microserviceapi | `NodePort` |
| microserviceapi.service.port | Port no. for microserviceapi  | `5000` |

### Grafana
| Parameter	  | Description | Default |
| ------      | ------      | ------ |             
| replicaCount | replica for your deployment | 1 |
| grafana.image.repository |image repository for grafana  | `grafana/grafana"6.2.1` |
| grafana.GfSecurityAdminUser | GfSecurityAdminUser for grafana | admin |
| grafana.GfSecurityAdminPassword | GfSecurityAdminPassword for grafana | admin |
| grafana.GfAuthAnonymousEnabled| GfAuthAnonymousEnabled for grafana | true |
| grafana.GfAuthAnonymousOrgRole| GfAuthAnonymousOrgRole for grafana | Admin|
| grafana.service.type | service type for grafana | `NodePort` |
| grafana.service.port | Port no. for grafana  | `3000` |

### prometheus
| Parameter	  | Description | Default |
| ------      | ------      | ------ |
| replicaCount | replica for your deployment | 1 |
| prometheus.image.repository |image repository for prometheus  | `prom/prometheus:v2.10.0` |
| prometheus.service.type | service type for prometheus | `NodePort` |
| prometheus.service.port | Port no. for prometheus | `9090` |

### database
| Parameter	  | Description | Default |
| ------      | ------      | ------ |
| replicaCount | replica for your deployment | 1 |
| database.image.repository | image repository for database | `stuartshay/microservice-database:v1` |
| database.POSTGRES_PASSWORD | postgrespassword value | password |
| database.service.port | Port no. for database  | `5432` |
| database.service.type | service type for database | `NodePort` |
