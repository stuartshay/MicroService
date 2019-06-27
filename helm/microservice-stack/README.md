# Introduction
This chart deploy all the deployments on a Kubernetes cluster/Minikube using the Helm package manager.


## Application Installed
The following packages will be installed by this helm chart
* Database
* Microservice-api
* Grafana
* Prometheus
* Alert-manager
* Mailhog

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
NAME                                           TYPE      CLUSTER-IP      EXTERNAL-IP  PORT(S)                        AGE
micro-api-microservice-stack-alertmanager      NodePort  10.100.122.183  <none>       9093:31834/TCP                 3m53s
micro-api-microservice-stack-database          NodePort  10.105.119.100  <none>       5432:30339/TCP                 3m53s
micro-api-microservice-stack-grafana           NodePort  10.107.148.155  <none>       3000:32113/TCP                 3m53s
micro-api-microservice-stack-mailhog           NodePort  10.105.222.146  <none>       8025:32288/TCP,1025:32152/TCP  3m53s
micro-api-microservice-stack-microservice-api  NodePort  10.105.109.137  <none>       5000:30437/TCP                 3m53s
micro-api-microservice-stack-prometheus        NodePort  10.99.231.53    <none>       9090:31704/TCP                 3m53s


```
To access microservice-api type NodeIP:port-no/swagger ( e.g nodeip:32583/swagger)

To access grafana type NodeIP:port-no ( e.g nodeip:31188)

To access prometheus type NodeIP:port-no ( e.g nodeip:30718)

To access alertmanager type NodeIP:port-no

To access mailhog type NodeIP:port-no
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

### Prometheus
| Parameter	  | Description | Default |
| ------      | ------      | ------ |
| replicaCount | replica for your deployment | 1 |
| prometheus.image.repository |image repository for prometheus  | `prom/prometheus:v2.10.0` |
| prometheus.service.type | service type for prometheus | `NodePort` |
| prometheus.service.port | Port no. for prometheus | `9090` |

### Database
| Parameter	  | Description | Default |
| ------      | ------      | ------ |
| replicaCount | replica for your deployment | 1 |
| database.image.repository | image repository for database | `stuartshay/microservice-database:v1` |
| database.POSTGRES_PASSWORD | postgrespassword value | password |
| database.service.port | Port no. for database  | `5432` |
| database.service.type | service type for database | `NodePort` |

## Alertmanager
| Parameter	  | Description | Default |
| ------      | ------      | ------ |
| replicaCount | replica for your deployment | 1 |
| alertmanager.image.repository | image repository for alertmanager | `prom/alertmanager:latest` |
| alertmanager.service.port | Port no. for alertmanager | `9093` |
| alertmanager.service.type | service type for alertmanager | `NodePort` |

## Mailhog
| Parameter	  | Description | Default |
| ------      | ------      | ------ |
| replicaCount | replica for your deployment | 1 |
| mailhog.image.repository | image repository for mailhog | `mailhog/mailhog` |
| mailhog.service.smtp.port | SMTP Port no. for mailhog | `1025` |
| mailhog.service.http.port | HTTP Port no. for mailhog | `8025` |
| mailhog.service.type | service type for mailhog | `NodePort` |
