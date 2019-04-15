#!/usr/bin/env bash
set -ex

PGUSER=postgres psql <<- EOSQL
   COPY public.test_data FROM '/data/test_data_201904121704.csv' DELIMITER ',' CSV HEADER;
EOSQL
