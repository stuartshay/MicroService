FROM prom/prometheus:v2.20.1

COPY  config/prometheus.yml  /etc/prometheus/prometheus.yml
COPY  config/  /etc/prometheus/targets/
COPY  alerts/  /etc/prometheus/

# Set Write Permission to override at runtime
USER root
RUN chmod 644 -c /etc/prometheus/prometheus.yml
