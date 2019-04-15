#!/usr/bin/env bash
set -ex

PGUSER=postgres psql <<- EOSQL
 CREATE TABLE public.test_data (
	id serial NOT NULL,
	"data" float8 NOT NULL,
	CONSTRAINT test_data_pkey PRIMARY KEY (id)
);
EOSQL


