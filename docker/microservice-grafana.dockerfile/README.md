# Navigator Grafana

[![This image on DockerHub](https://img.shields.io/docker/pulls/stuartshay/navigator-grafana.svg)](https://hub.docker.com/r/stuartshay/navigator-grafana/) [![](https://images.microbadger.com/badges/image/stuartshay/navigator-grafana.svg)](https://microbadger.com/images/stuartshay/navigator-grafana "Get your own image badge on microbadger.com")
[![](https://images.microbadger.com/badges/version/stuartshay/navigator-grafana.svg)](https://microbadger.com/images/stuartshay/navigator-grafana "Get your own version badge on microbadger.com")

Jenkins | Status  
------------ | -------------
Build Image  | [![Build Status](https://jenkins.navigatorglass.com/buildStatus/icon?job=Navigator-Infrastructure/navigator-grafana)](https://jenkins.navigatorglass.com/job/Navigator-Infrastructure/job/navigator-grafana/)

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
