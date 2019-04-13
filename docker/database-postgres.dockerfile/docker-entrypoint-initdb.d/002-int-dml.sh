#!/usr/bin/env bash
set -ex

psql -U postgres --command "CREATE TABLE public.test_data (	id serial NOT NULL, 'data' float8 NOT NULL, CONSTRAINT test_data_pkey PRIMARY KEY (id);"
psql -U postgres --command "CREATE OR REPLACE FUNCTION public.get_data() RETURNS SETOF double precision LANGUAGE plpgsql STABLE AS $function$ BEGIN RETURN QUERY SELECT data FROM public.test_data; END $function$;"

