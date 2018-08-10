using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prometheus;
using System.Threading;

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

        public void Init(HttpApplication application)
        {
            application.PreSendRequestContent += new EventHandler(CollectMetricsWhenRequest);
        }

        private void CollectMetricsWhenRequest(Object source,
             EventArgs e)
        {
            if (!PromServer.Instance.isServerOpen())
            {
                Console.Write("Server haven't been opened yet.");
                return;
            }

            HttpApplication application = (HttpApplication)source;
            var context = application.Context;
            string filePath = context.Request.FilePath;
            string[] loc = filePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string fileExtension = VirtualPathUtility.GetExtension(filePath);

            if (!(loc.Length > 0 && loc[0].Substring(0, 1).Equals("_")))
            {

                /* START STATEMENT
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

                /*
                 * 
                 * 
                 * END STATEMENT 
                 */
            }
        }

        public static List<int> intervals = new List<int> { 60*1000 };
        public static void StartAllThread()
        {
            foreach (int interval in intervals)
            {
                Thread thread = new Thread(CallThreading);
                thread.Start(interval);
            }
        }

        private static void CallThreading(object interval)
        {
            while (PromServer.Instance.isServerOpen())
            {
                /* START STATEMENT
                 * 
                 * IN THIS STATEMENT IS EQUAL TO INTERVAL FUNCTION FROM NODEJS
                 * You should add all metrics collector in here.
                 * THIS STATEMENT collect every 1 minutes.
                 * if you wanna edit this interval, you can edit it on intervals variable
                 * 
                 */

                Counter c = Metrics.CreateCounter("counterThread", "help", new string[] { "interval" });
                c.Labels(interval.ToString()).Inc();

                /*
                 * 
                 * 
                 * END STATEMENT
                 */
                Thread.Sleep(int.Parse(interval.ToString()));
            }
        }

        public void Dispose() { }
    }
}