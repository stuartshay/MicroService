FROM prom/prometheus:v2.30.3

COPY --chmod=0644 config/prometheus.yml  /etc/prometheus/prometheus.yml
COPY  config/  /etc/prometheus/targets/
COPY  alerts/  /etc/prometheus/

USER nobody
