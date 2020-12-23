#!/usr/bin/env bash
set -ex

FILE=/data/nyc_pluto_20v7_csv.zip
if [ ! -f "$FILE" ]; then
    echo "File not found!"
fi



#curl -sS https://www1.nyc.gov/assets/planning/download/zip/data-maps/open-data/nyc_pluto_20v7_csv.zip > /data/nyc_pluto_20v7_csv.zip
#unzip /data/nyc_pluto_20v7_csv.zip