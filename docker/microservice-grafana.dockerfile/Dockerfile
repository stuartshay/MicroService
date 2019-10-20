FROM grafana/grafana:6.2.5

COPY  docker/microservice-grafana.dockerfile/scripts/grafana.sh /usr/local/bin
#COPY ./scripts/grafana.sh /usr/local/bin

COPY  docker/microservice-grafana.dockerfile/provisioning /etc/grafana/provisioning
##COPY  ./provisioning /etc/grafana/provisioning








RUN pwd
RUN ls





##COPY scripts /app/src


#USER root




#COPY ./scripts/grafana.sh /usr/local/bin

#ADD ./provisioning /etc/grafana/provisioning
#ADD ./config.ini /etc/grafana/config.ini
## ADD ./dashboards /var/lib/grafana/dashboards

#RUN apt-get update && \
#    apt-get install --no-install-recommends less && \
#    rm -rf /var/lib/apt/lists/* 
#    ##  && \
#    ##chmod +x /usr/local/bin/grafana.sh 


##RUN /usr/local/bin/grafana.sh
