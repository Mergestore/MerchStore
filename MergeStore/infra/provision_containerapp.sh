#!/bin/bash

resource_group=CloudSoftRG
location=northeurope
env_name=PearEnv
app_name=pearapp
app_port=8080
image=ghcr.io/monswiklund/Pear

az group create --location $location --name $resource_group

az containerapp env create --name $env_name --resource-group $resource_group --location $location

az containerapp create --name $app_name --resource-group $resource_group \
                       --image $image \
                       --environment $env_name \
                       --target-port $app_port \
                       --ingress external --query properties.configuration.ingress.fqdn