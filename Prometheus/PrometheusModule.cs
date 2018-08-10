using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prometheus;

namespace Prometheus.Custom
{
    public class PrometheusModule : IHttpModule
    {

        PrometheusModule()
        {
        }

        public String ModuleName
        {
            get { return "PrometheusModule"; }
        }

        // In the Init function, register for HttpApplication 
        // events by adding your handlers.
        public void Init(HttpApplication application)
        {
            application.PreSendRequestContent += new EventHandler(CollectMetricsWhenRequest);
        }

        private void CollectMetricsWhenRequest(Object source,
             EventArgs e)
        {
            //Check if prometheus server is opened.
            if (!PromServer.Instance.isServerOpen())
            {
                Console.Write("Server haven't been opened yet.");
                return;
            }

            //Split content of user's request
            HttpApplication application = (HttpApplication)source;
            var context = application.Context;
            string filePath = context.Request.FilePath;
            string[] loc = filePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string fileExtension = VirtualPathUtility.GetExtension(filePath);


            //Collect all request to prometheus
            //Condition: Request save only filepath to project. Not other browser data. (For example: /__browserLink/...)
            if (!(loc.Length > 0 && loc[0].Substring(0, 1).Equals("_")))
            {

                /*
                 * 
                 * IN THIS STATEMENT IS EQUALS TO MIDDLEWARE FROM NODEJS.
                 * You should add all metrics collector in here.
                 *
                 * CRATECOUNTER AND OTHER CREATECOLLECTORS ARE WELL-DONE HERE!
                 * Not duplicate when name is the same as the last one.
                 * 
                */
                Counter c = Metrics.CreateCounter("counterCall", "help", new string[] { "file", "status" });
                c.Labels(context.Request.FilePath, context.Response.Status).Inc();
            }
        }

        public void Dispose() { }
    }
}