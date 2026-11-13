FROM prom/prometheus:v3.16.0

COPY --chmod=0644 config/prometheus.yml  /etc/prometheus/prometheus.yml
COPY  config/  /etc/prometheus/targets/
COPY  alerts/  /etc/prometheus/

USER nobody
