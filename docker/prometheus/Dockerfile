FROM prom/prometheus:v2.55.1

COPY --chmod=0644 config/prometheus.yml  /etc/prometheus/prometheus.yml
COPY  config/  /etc/prometheus/targets/
COPY  alerts/  /etc/prometheus/

USER nobody
