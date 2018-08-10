using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Web;
using Prometheus.Advanced;
using Prometheus.Advanced.DataContracts;
using Prometheus;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Prometheus.Custom
{
    [Serializable()]
    public class InvalidHeader : System.Exception
    {
        public InvalidHeader() : base() { }
        public InvalidHeader(string message) : base(message) { }
        public InvalidHeader(string message, System.Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected InvalidHeader(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
        { }
    }

    public class MetricsCustomServer : IDisposable
    {
        private readonly HttpListener _httpListener = new HttpListener();
        private Task _task;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private ICollectorRegistry _registry;

        private int port;
        private string url;
        private ICollectorRegistry registry;
        private bool useHttps;
        private string hostname;
        private string authMode = "None";

        //Basic auth content : little alphabets are the meaning.
        string iCECREAMdONUT = "root";
        string pOwERdOG = "root";

        /// <summary>
        /// Custom metrics server without basic auth
        /// </summary>
        public MetricsCustomServer(int port, string hostname = "localhost", string url = "metrics/", ICollectorRegistry registry = null, bool useHttps = false)
        {
            this.hostname = hostname;
            this.port = port;
            this.url = url;
            this.registry = registry;
            this.useHttps = useHttps;
            var s = useHttps ? "s" : "";
            _httpListener.Prefixes.Add($"http{s}://{hostname}:{port}/{url}/");
            _registry = registry ?? DefaultCollectorRegistry.Instance;
        }

        /// <summary>
        /// Custom metrics server with basic auth
        /// </summary>
        public MetricsCustomServer(int port, string basic_id, string basic_pwd, string hostname = "localhost", string url = "metrics/", ICollectorRegistry registry = null, bool useHttps = false) : 
            this(port,hostname:hostname,url: url,registry: registry,useHttps: useHttps)
        {
            authMode = "basic";
            iCECREAMdONUT = basic_id;
            pOwERdOG = basic_pwd;
            _httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;
        }

        /// <summary>
        /// This method is called to start metrics server
        /// </summary>
        public void Start()
        {
            if (_task != null)
                throw new InvalidOperationException("The metric server has already been started.");

            _task = StartServer(_cts.Token);
        }

        private Task StartServer(CancellationToken cancel)
        {
            _httpListener.Start();

            return Task.Factory.StartNew(delegate
            {
                try
                {
                    while (!cancel.IsCancellationRequested)
                    {
                        var getContext = _httpListener.GetContextAsync();
                        getContext.Wait(cancel);
                        var context = getContext.Result;
                        var request = context.Request;
                        var response = context.Response;

                        try
                        {
                            if (authMode == "None" || CheckAuthenticate(context))
                            {
                                IEnumerable<MetricFamily> metrics;
                                metrics = _registry.CollectAll();

                                var acceptHeader = request.Headers.Get("Accept");
                                var acceptHeaders = acceptHeader?.Split(',');
                                var contentType = ScrapeHandler.GetContentType(acceptHeaders);
                                response.ContentType = contentType;

                                response.StatusCode = 200;

                                using (var outputStream = response.OutputStream)
                                    ScrapeHandler.ProcessScrapeRequest(metrics, contentType, outputStream);
                            }
                            else
                            {
                                throw new InvalidHeader();
                            }
                        }
                        catch (InvalidHeader ex)
                        {
                            response.StatusCode = 404;
                            //tell something about invalid header
                        }
                        catch (Exception ex) when (!(ex is OperationCanceledException))
                        {
                            Trace.WriteLine(string.Format("Error in MetricsServer: {0}", ex));
                            response.StatusCode = 404;
                        }
                        finally
                        {
                            response.Close();
                        }
                    }
                }
                finally
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                }
            }, TaskCreationOptions.LongRunning);
        }

        private bool CheckAuthenticate(HttpListenerContext context)
        {
            if (authMode == "basic")
            {
                var header = (HttpListenerBasicIdentity)context.User.Identity;
                var id = header.Name;
                var pwd = header.Password;
                return id.Equals(iCECREAMdONUT) && pwd.Equals(pOwERdOG);
            }
            return false;
        }

        /// <summary>
        /// This method is called to stop metrics server.
        /// </summary>
        public void Stop()
        {
            StopAsync().GetAwaiter().GetResult();
        }

        private async Task StopAsync()
        {
            _cts?.Cancel();

            try
            {
                if (_task == null)
                    return;
                await _task;
            }
            catch (OperationCanceledException)
            {

            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}