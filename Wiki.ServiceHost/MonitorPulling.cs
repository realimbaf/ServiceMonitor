using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using CarParts.Common.Log;
using Wiki.Service.Common;
using Wiki.Service.Common.Clients;
using Wiki.ServiceHost.Nuget;

namespace Wiki.ServiceHost
{
    internal class MonitorPulling
    {
        private int _interval = 1000;
        private readonly FileLogger _logger;
        private readonly ServiceMonitorClient _client;
        private readonly ServiceInfo _info;
        private bool _isStop;

        public MonitorPulling(PackageInfo package, string url, FileLogger logger)
        {
            this._logger = logger;
            this._client = new ServiceMonitorClient();

            this._info = new ServiceInfo
            {
                Id = package.Id,
                Version = package.Version,
                Url = url.Replace("+", Dns.GetHostName()),
                ProcessId = Process.GetCurrentProcess().Id
            };
        }

        public void Start()
        {
            this._logger.WriteEvent("Pulling start");
            this.StartPulling();
            TurnOnTimer();
        }

        private void StartPulling()
        {
            this._isStop = false;
            var th = new Thread(PullWork);
            th.Start();
        }

        private void PullWork()
        {
            while (!this._isStop)
            {
                try
                {
                    var result = this._client.Register(this._info);
                    if (!result)
                    {
                        this._logger.WriteWarning("Service is stoping (pulling false). ");
                        MainSrv.StopAsync();
                        return;
                    }
                }
                catch (Exception e)
                {

                    if (e is AggregateException && e.InnerException != null)
                        e = e.InnerException;
                    if (e.InnerException != null)
                        e = e.InnerException;
                    if ((DateTime.Now - this._lastLogError).TotalMinutes > 1)
                    {
                        this._logger.WriteEvent("Pooling error:{0}", e.ToString());
                        this._lastLogError = DateTime.Now;
                    }

                }

                Thread.Sleep(this._interval);

            }
        }

        public void Stop()
        {
            this._logger.WriteEvent("Pulling stop");
            this._isStop = true;
        }

        private static void TurnOnTimer()
        {
        }

        private DateTime _lastLogError ;              
    }
}