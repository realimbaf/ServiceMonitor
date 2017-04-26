using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using CarParts.Common.Log;
using Microsoft.Owin.Hosting;
using Wiki.Service.Common.Clients;
using Wiki.Service.Configuration;
using Wiki.ServiceMonitor.Monitor;
using Wiki.ServiceMonitor.MonitorDiscovery;

namespace Wiki.ServiceMonitor
{
    public class MonitorSrv : IDisposable
    {
        internal const string MonitorUrl = "http://+:9100/";
        private static readonly FileLogger _logger = new FileLogger("mon");
        private static MonitorSrv _instanse;
        private IDisposable _srv;
        private readonly ManualResetEventSlim _stopSwitch = new ManualResetEventSlim();
        private readonly MonitorRelationContext _broadcastToDiscovery = new MonitorRelationContext();
        private readonly ActiveServiceMonitor _monitor;

        public MonitorSrv()
        {
            _instanse = this;
            this._monitor = new ActiveServiceMonitor(_logger, new ServiceManager(_logger));
        }

        public static FileLogger Logger
        {
            get { return _logger; }
        }

        internal static ActiveServiceMonitor Monitor { get { return _instanse._monitor; } }

        public static string RepositoryPath
        {
            get { return ConfigurationManager.AppSettings["nugetPath"]; }
        }

        public void Dispose()
        {
            Stop();
        }

        private void StartWeb()
        {
            var url = MonitorUrl;
            //NetAclChecker.AddAddress(url);
            this._srv = WebApp.Start<Startup>(url);
            _logger.WriteEvent("Service start. url:{0}", url);
        }

        public void Start()
        {
            DiscoveryFactory.SetDefaultCreditenals(new ClientCreditenals { ClientId = "service", ClientSecret = "servicepasswd" });
            ConfigurationContainer.SetBaseDir(Environment.CurrentDirectory);

            _logger.WriteEvent("*****Service start******");

            StartWeb();
            this._monitor.Start();
            this._broadcastToDiscovery.Start();
            //Call udp broadcast
        }

        public void Stop()
        {
            _logger.WriteEvent("*****Stop web******");

            if (this._srv != null)
                this._srv.Dispose();
            this._monitor.Stop();

            this._stopSwitch.Set();
        }

        public static void StopAsync()
        {
            if (_instanse != null)
                Task.Factory.StartNew(() => _instanse.Stop());
        }

        public void Wait()
        {
            this._stopSwitch.Wait();
        }
    }
}