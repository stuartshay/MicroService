FROM grafana/grafana:8.2.2-ubuntu

COPY ./scripts/grafana.sh /usr/local/bin
COPY  config.ini /etc/grafana/config.ini
COPY  ./provisioning /etc/grafana/provisioning

USER root
RUN apt-get update && \
    apt install nano && \
    apt install net-tools && \
    apt install tree && \
    apt-get install --no-install-recommends less && \
    rm -rf /var/lib/apt/lists/*  && \
    chmod +x /usr/local/bin/grafana.sh 

RUN /usr/local/bin/grafana.sh