FROM osgeo/gdal

COPY ./scripts/  /scripts/
COPY ./scripts/from_shapefile  /scripts/from_shapefile
COPY ./scripts/to_GeoJSON  /scripts/to_GeoJSON 

WORKDIR /scripts
 
ENTRYPOINT [ "python" ]

CMD [ "app.py" ]



