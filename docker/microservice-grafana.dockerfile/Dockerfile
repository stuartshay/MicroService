FROM grafana/grafana:6.2.5

USER root

ADD ./scripts/grafana.sh /usr/local/bin

#ADD ./provisioning /etc/grafana/provisioning
#ADD ./config.ini /etc/grafana/config.ini
## ADD ./dashboards /var/lib/grafana/dashboards

RUN chmod +x /usr/local/bin/grafana.sh && \
    apt-get update && \
    apt-get install --no-install-recommends less && \
    rm -rf /var/lib/apt/lists/* 

RUN /usr/local/bin/grafana.sh
