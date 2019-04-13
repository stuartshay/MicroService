FROM postgres:9.6

COPY scripts  /scripts
COPY data  /data

COPY docker-entrypoint-initdb.d /docker-entrypoint-initdb.d

RUN echo "host all  all    0.0.0.0/0  md5" >> /var/lib/postgresql/data/pg_hba.conf && \
    echo "listen_addresses='*'" >> /var/lib/postgresql/data/postgresql.conf 

