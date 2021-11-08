FROM osgeo/gdal

COPY scripts  /scripts

USER root
RUN chmod +x /scripts/app.sh


ENTRYPOINT ["/scripts/app.sh"]

