#!/usr/bin/env bash
set -ex
psql -U postgres --command "CREATE USER development WITH SUPERUSER PASSWORD 'development';"
