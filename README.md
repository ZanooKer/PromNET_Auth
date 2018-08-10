# PromNET_Auth

# Requirement:
Prometheus_net v.1.3.5 (Because I dev this proj on VS2015)

# Import Step:
1. Nuget import package Prometheus_net as description above
2. Download Prometheus folder to root folder of the project 
3. Edit web.config / Main web-config by adding
   
   3.1 Add Prometheus Module to httpModule : adding below code between 'configuration' outline
      adding below code between 'system.web -> httpModules' outline 
   
   ```javascript
   <add name="prometheusMod" type="Prometheus.Custom.PrometheusModule" />
   ```
   
   also adding this between 'system.webServer -> modules' outline to run this module with IIS7.0
   
   ```javascript
   <remove name="prometheusMod" />
   <add name="prometheusMod" type="Prometheus.Custom.PrometheusModule" />
   ```

   3.2 Add reference to Prometheus Config : adding below code between 'configuration' outline
   
      ```javascript
   <configSections>
    <section name="PromSet"
         type="System.Configuration.DictionarySectionHandler"/>
   </configSections>
   <PromSet configSource ="Prometheus\Prom.config" />
   ```
   
4. In Global.asax.cs, Add "PromServer.Instance.Init()" in Application_Start function.
   
   (PromServer's full name : Prometheus.Custom.Promserver)

# Editing to your likes!
1. Change your prometheus server's setting : Editing only at Prometheus/Prom.config
2. Change metrics to sent on prometheus server : Editing the field in Prometheus/PrometheusModule.cs
   
