using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Prometheus;
using System.Text;
using System.Reflection;
using System.IO;
using System.Net;
using System.Configuration;
using System.Collections;
using System.Web.Configuration;

namespace Prometheus.Custom
{
    public class PromServer
    {
        private static PromServer instance = null;
        private static readonly object padlock = new object();
        private static MetricsCustomServer server;
        private static bool initialPort = false;

        /*Default path for exporting prometheus metrics*/
        private string hostname = "localhost";
        private int port = 8000;
        private string metricsPath = "metrics";
        private List<string> authCont = new List<string>();

        PromServer()
        {
            initialPort = false;
        }

        /// <summary>
        /// Prometheus server is singleton. This is the instance of prometheus server.
        /// </summary>
        public static PromServer Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new PromServer();
                        instance.ReadConfig();
                    }
                    return instance;
                }
            }
        }
        /// <summary>
        /// This method use to set location of prometheus metrics by loading from PromConfig.txt
        /// </summary>
        private void ReadConfig()
        {
            var section = (Hashtable)ConfigurationManager.GetSection("PromSet");
            Dictionary<string, string> dictionary = section.Cast<DictionaryEntry>().ToDictionary(d => (string)d.Key, d => (string)d.Value);
            instance.hostname = dictionary["hostname"];
            instance.port = int.Parse(dictionary["port"]);
            instance.metricsPath = dictionary["path"];
            if (dictionary["authType"].Equals("none")) return;
            if (dictionary["authType"].Equals("basic"))
            {
                authCont.Add("basic");
                if (string.IsNullOrEmpty(dictionary["basicID"]))
                    authCont.Add(dictionary["basicID"]);
                else
                    authCont.Add("root");
                if (string.IsNullOrEmpty(dictionary["basicPwd"]))
                    authCont.Add(dictionary["basicPwd"]);
                else
                    authCont.Add("root");

                return;
            }
            Console.WriteLine("Error: AuthType isn't basic or none. Please fix it at Prom.config");
        }

        /// <summary>
        /// This method use to start server.
        /// </summary>
        public void Init()
        {
            if (!initialPort)
            {
                if (authCont.Count > 0)
                {
                    if (authCont[0].Equals("basic"))
                    {
                        server = new MetricsCustomServer(port,
                            basic_id: authCont[1],
                            basic_pwd: authCont[2],
                            hostname: hostname,
                            url: metricsPath);
                    }
                }
                else
                {
                    server = new MetricsCustomServer(port,
                        hostname: hostname,
                        url: metricsPath);
                }
                server.Start();
                initialPort = true;
            }
        }

        /// <summary>
        /// This method use to stop server.
        /// </summary>
        public void Exit()
        {
            if (initialPort)
            {
                server.Stop();
                initialPort = false;
            }
        }

        /// <summary>
        /// This methos use to check if server is opened.
        /// </summary>
        public bool isServerOpen()
        {
            return initialPort;
        }
    }
}