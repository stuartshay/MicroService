FROM grafana/grafana:6.2.5

COPY  docker/microservice-grafana.dockerfile/scripts/grafana.sh /usr/local/bin
COPY  docker/microservice-grafana.dockerfile/provisioning /etc/grafana/provisioning
COPY docker/microservice-grafana.dockerfile/dashboards /var/lib/grafana/dashboards

##COPY ./scripts/grafana.sh /usr/local/bin
##COPY  ./provisioning /etc/grafana/provisioning
##COPY ./dashboards /var/lib/grafana/dashboards

RUN pwd


USER root
RUN apt-get update && \
    apt-get install --no-install-recommends less && \
    rm -rf /var/lib/apt/lists/*  


RUN chmod +x /usr/local/bin/grafana.sh 

RUN /usr/local/bin/grafana.sh
