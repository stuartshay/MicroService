#!/usr/bin/env bash
set -ex

#curl -sS https://www1.nyc.gov/assets/planning/download/zip/data-maps/open-data/nyc_pluto_20v7_csv.zip > /data/nyc_pluto_20v7_csv.zip
#unzip /data/nyc_pluto_20v7_csv.zip

PGUSER=postgres psql <<- EOSQL
   COPY public.test_data FROM '/data/test_data_201904121704.csv' DELIMITER ',' CSV HEADER;
EOSQL
