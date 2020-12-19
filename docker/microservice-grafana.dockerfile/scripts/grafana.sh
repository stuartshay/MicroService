#!/bin/sh

## Plugins 
grafana-cli plugins install grafana-piechart-panel
grafana-cli plugins install grafana-clock-panel
grafana-cli plugins install briangann-datatable-panel
grafana-cli plugins install briangann-gauge-panel
grafana-cli plugins install bessler-pictureit-panel
grafana-cli plugins install michaeldmoore-multistat-panel
grafana-cli plugins install mtanda-histogram-panel
grafana-cli plugins install novalabs-annotations-panel
grafana-cli plugins install ryantxu-ajax-panel
grafana-cli plugins install snuids-trafficlights-panel
grafana-cli plugins install yesoreyeram-boomtable-panel

## Datasource
grafana-cli plugins install grafana-simple-json-datasource

## App
grafana-cli plugins install raintank-worldping-app
