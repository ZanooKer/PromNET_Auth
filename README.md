# PromNET_Auth

# Requirement:
Prometheus_net v.1.3.5 (Because I dev this proj on VS2015)

# Import Step:
1. Nuget import package Prometheus_net as description above
2. Download Prometheus folder to root folder of the project 
3. Edit web.config / Main web-config by adding
   3.1 Add Prometheus Module to httpModule
   3.2 Add reference to Prometheus Config

# Editing to your likes!
1. Change your prometheus server's setting : Editing only at Prometheus/Prom.config
2. Change metrics to sent on prometheus server : Editing the field in Prometheus/PrometheusModule.cs
   
