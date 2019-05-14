# Helm Chart


## Introduction
This chart deploy all the deployments on a Kubernetes cluster using the Helm package manager.


## Application Installed
The following packages will be installed by this helm chart
* api-navigator-attractions
* api-navigator-maps
* api-navigator-reports
* cadvisor
* elasticsearch
* grafana
* graylog
* load-locust
* mongo
* mongo-rs1
* mongodb-exporter
* prometheus
* redis-exporter
* redis
* web-maps

## Prerequisite
* Kubernetes
* Helm

## Installing the Chart
To install the chart
1. Clone the repo
2. Run the below command

```sh
$ helm install --name=name of the chart -f values.yaml .
```
Confirm the deployment by, running the command:

```sh
$ helm ls
```

## Uninstalling the Chart

To uninstall/delete the all the deployments

```sh
$ helm del --purge chart name
```



## Configuration

The following table lists the configurable parameters for this helm chart and their default values set.

| Parameter	  | Description | Default |
| ------ | ------ | ------ |
| replicaCount | replica for your deployment | 1
| apinavigatorattractions.image |image repository for api-navigator-attractions  | `stuartshay/navigator-attractions-api:2.2.3-local` |
| apinavigatormaps.image| image repository for api-navigator-maps | `stuartshay/navigator-maps-api:2.2-3-local` |
| apinavigatorreports.image |image repository for apinavigatorreports  | `stuartshay/navigator-reports-api:2.2.3-local` |
| cadvisor.image |image repository for cadvisor  | `google/cadvisor:v0.33.0` |
| elasticsearch.image |image repository for elasticsearch  | `docker.elastic.co/elasticsearch/elasticsearch-oss:6.6.1` |
| graylog.image |image repositor for graylog   | `graylog/graylog:3.0`|
| grafana.image |image repository for grafana  | `stuartshay/coredatastore-grafana:6.1.3-v3` |
| load locust.image |image repository for load locust  | `stuartshay/navigator-locust:v0.11.0-v5` |
| mongo.image |image repository for mongo   | `mongo:3` |
| mongors.image |image repository for mongors-1   | `stuartshay/navigator-instance:3.6.11-v2` |
| mongodbexporter.image |image repository for mongodb-exporter  | `stuartshay/navigator-mongodb-exporter:v1.0.0` |
| prometheus.image |image repository for prometheus  | `stuartshay/coredatastore-prometheus:v2.8.1-v1` |
| redis.image |image repository for redis  | `stuartshay/coredatastore-redis:local-v1` |
| redis.exporter |image repository for redis exporter  | `oliver006/redis_exporter:v0.32.0` |
| web-maps |image repository for web-maps  | `stuartshay/navigator-maps:1.0.4-local` |
| service.type | service type  | `NodePort` |
| apinavigatormaps.service.ports.port | Port no. for api-navigator-maps  | `7000` |
| apinavigatorattractions.service.ports.port | Port no. for api-navigator-attractions  | `7100` |
| apinavigatorreportss.service.ports.port | Port no. for api-navigator-reports  | `7200` |
| cadvisor.service.ports.port | Port no. for cadvisor  | `8080` |
| elasticsearch.service.ports.port | Port no. for elasticsearch  | `9200` |
| grafana.service.ports.port | Port no. for grafana  | `3000` |
| graylog.service.ports.port9000 | Port no. for graylog  | `9000` |
| graylog.service.ports.port1514 | Port no. for graylog  | `1514` |
| graylog.service.ports.port12201 | Port no. for graylog  | `12201` |
| load-locust.service.ports.port | Port no. for load-locust  | `8089` |
| mongo.service.ports.port | Port no. for mongo  | `27017` |
| mongors.service.ports.port | Port no. for mongo-rs1  | `27017` |
| mongodbexporter.service.ports.port | Port no. for mongodb-exporter  | `9001` |
| prometheus.service.ports.port | Port no. for prometheus  | `9090` |
| redis.service.ports.port | Port no. for redis  | `6379` |
| redisexporter.service.ports.port | Port no. for redis-exporter  | `9121` |
| webmaps.service.ports.port | Port no. for web-maps  | `3200` |
