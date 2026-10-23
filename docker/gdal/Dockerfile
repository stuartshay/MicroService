FROM osgeo/gdal:ubuntu-small-3.6.3

COPY ./scripts/  /scripts/

RUN mkdir -p /scripts/from_shapefile /scripts/to_GeoJSON && \
    groupadd --gid 10001 gdal && \
    useradd --uid 10001 --gid gdal --no-create-home gdal && \
    chown -R gdal:gdal /scripts

USER gdal
WORKDIR /scripts
 
ENTRYPOINT [ "python3" ]

CMD [ "app.py" ]



