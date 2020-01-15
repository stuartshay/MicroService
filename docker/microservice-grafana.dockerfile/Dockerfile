FROM grafana/grafana:6.5.3

COPY  docker/microservice-grafana.dockerfile/scripts/grafana.sh /usr/local/bin
COPY  docker/microservice-grafana.dockerfile/config.ini /etc/grafana/config.ini
COPY  docker/microservice-grafana.dockerfile/provisioning /etc/grafana/provisioning
COPY  docker/microservice-grafana.dockerfile/dashboards /var/lib/grafana/dashboards

RUN ls

##COPY ./scripts/grafana.sh /usr/local/bin
##COPY  config.ini /etc/grafana/config.ini
##COPY  ./provisioning /etc/grafana/provisioning
##COPY ./dashboards /var/lib/grafana/dashboards

USER root
RUN apt-get update && \
    apt-get install --no-install-recommends less && \
    rm -rf /var/lib/apt/lists/*  && \
    chmod +x /usr/local/bin/grafana.sh 

RUN /usr/local/bin/grafana.sh
