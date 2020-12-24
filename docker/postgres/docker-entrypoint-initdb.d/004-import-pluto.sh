#!/usr/bin/env bash
set -ex

FILE=/data/nyc_pluto_20v7_csv.zip
DESTINATION=/data/pluto

mkdir -p $DESTINATION

if [ ! -f "$FILE" ]; then
    curl -sS https://www1.nyc.gov/assets/planning/download/zip/data-maps/open-data/nyc_pluto_20v7_csv.zip > $FILE
    unzip $FILE -d $DESTINATION
fi

PGUSER=postgres psql <<- EOSQL
   COPY public.pluto FROM '/data/pluto/pluto_20v7.csv' DELIMITER ',' CSV HEADER;
EOSQL

