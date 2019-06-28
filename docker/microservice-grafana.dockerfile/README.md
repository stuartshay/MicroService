# Microservice Grafana

[![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/microservice-grafana.svg)](https://hub.docker.com/r/stuartshay/microservice-grafana/) [![](https://images.microbadger.com/badges/image/stuartshay/microservice-grafana.svg)](https://microbadger.com/images/stuartshay/microservice-grafana "Get your own image badge on microbadger.com")
[![](https://images.microbadger.com/badges/version/stuartshay/microservice-grafana.svg)](https://microbadger.com/images/stuartshay/microservice-grafana "Get your own version badge on microbadger.com")

Jenkins | Status  
------------ | -------------
Build Image  | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=MicroService/microservice-grafana)](https://jenkins.navigatorglass.com/job/MicroService/job/microservice-grafana/)

```
├── docker-grafana
│   ├── Dockerfile              # Dockerfile
│   ├── config.ini              # Grafana config
|   |
│   ├── dashboards              # Grafana dashboards
│   │   └── dashboards.json
|   │
│   └── provisioning
│       ├── dashboards
│       │   └── all.yml         # Configuration dashboard provisioning
│       └── datasources
│           └── all.yml         # Configuration datasources provisioning
|
│   └── scripts
│       └── grafana.sh          # Grafana cli configuration
```
