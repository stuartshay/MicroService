FROM postgres:11.2

RUN apt-get update && apt-get install -y \
    curl \ 
    unzip \
    && rm -rf /var/lib/apt/lists/* \
    && curl https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh > /wait_for_it.sh \
    && chmod +x /*.sh 

COPY data  /data

COPY docker-entrypoint-initdb.d /docker-entrypoint-initdb.d
RUN chmod +x /docker-entrypoint-initdb.d/*.sh

RUN echo "host all  all    0.0.0.0/0  md5" >> /var/lib/postgresql/data/pg_hba.conf && \
    echo "listen_addresses='*'" >> /var/lib/postgresql/data/postgresql.conf 

