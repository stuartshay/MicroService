FROM prom/prometheus:v2.10.0

#COPY  config/prometheus.yml  /etc/prometheus/prometheus.yml
#COPY  config/  /etc/prometheus/targets/
#COPY  alerts/  /etc/prometheus/

COPY  docker/microservice-prometheus.dockerfile/config/prometheus.yml  /etc/prometheus/prometheus.yml
COPY  docker/microservice-prometheus.dockerfile/config/  /etc/prometheus/targets/
COPY  docker/microservice-prometheus.dockerfile/alerts/  /etc/prometheus/


# Set Write Permission to override at runtime
USER root
RUN chmod 644 -c /etc/prometheus/prometheus.yml
