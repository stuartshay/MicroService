# SonarQube

Run `make sonar` with local `SONAR_HOST_URL` and `SONAR_TOKEN` settings in `.env`.
The script builds Release, collects coverage, and submits analysis for
`MicroService.Api`. Never commit local settings or scanner output.

## Container review

The API explicitly runs as `app`. GDAL, Grafana, Prometheus, and the custom
NGINX image also run as non-root users. When bind-mounting files into these
containers, grant their runtime users access to the mounted files. In particular,
NGINX needs read access to its TLS certificate and private key and write access
to any mounted log directory. Keep private keys restricted to the required user
or group; do not make production private keys world-readable.

The PostgreSQL image intentionally keeps its official entrypoint's startup user.
It prepares mounted data-directory ownership as root and then drops privileges
to `postgres` before starting the database. Forcing `USER postgres` would bypass
that ownership repair and can break existing volumes. The corresponding `S6471`
finding needs a contextual security review; it is not globally suppressed.
See the [official PostgreSQL image documentation](https://github.com/docker-library/docs/blob/master/postgres/content.md).

Coverage tests exercise cache refresh and file-change handling with temporary
shapefiles, scheduler cancellation and disposal with a controlled clock, and
version health-check metadata. Mapping tests run with US, French, and Saudi
cultures to ensure file data does not depend on the host's culture.
