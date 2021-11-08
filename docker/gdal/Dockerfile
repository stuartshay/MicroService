FROM osgeo/gdal


COPY ./scripts/app.sh /scripts

USER root
RUN apt-get update && \
    apt install nano && \
    apt install net-tools && \
    apt install tree && \
    apt-get install --no-install-recommends less && \
    rm -rf /var/lib/apt/lists/*  && \
    chmod +x /scripts/app.sh


ENTRYPOINT ["/scripts/app.sh"]