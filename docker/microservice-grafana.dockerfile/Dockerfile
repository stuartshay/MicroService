FROM grafana/grafana:7.3.5-ubuntu


COPY ./scripts/grafana.sh /usr/local/bin
COPY  config.ini /etc/grafana/config.ini
COPY  ./provisioning /etc/grafana/provisioning
COPY ./dashboards /var/lib/grafana/dashboards

USER root
RUN apt-get update && \
    apt-get install --no-install-recommends less && \
    rm -rf /var/lib/apt/lists/*  && \
    chmod +x /usr/local/bin/grafana.sh 

RUN /usr/local/bin/grafana.sh
