using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Management.Instrumentation;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CarParts.Common.Log;
using Wiki.Service.Common;
using Wiki.Service.Common.Clients;

namespace Wiki.ServiceMonitor.Monitor
{
    internal class ServiceManager : IServiceManager
    {
        private readonly FileLogger _logger;
        private readonly ServiceCollection _services;

        public ServiceManager(FileLogger logger)
        {
            this._logger = logger;
            this._services = new ServiceCollection();
        }


        public async Task Stop(string id)
        {
            var service = this._services[id];
            if (service == null)
                throw new KeyNotFoundException(string.Format("Service '{0}' not found", id));
            if (!service.IsRun)
                throw new InstanceNotFoundException(string.Format("Service '{0}' is stoped", id));
            service.IsRun = false;

            this._logger.WriteEvent("Sent to service host stop command. Service id:{0}, process id:{1}",service.Id,service.ProcessId);

            var token = await DiscoveryFactory.GetToken(Startup.GetIdentityServerUrl());

            var cl = new HttpClient();
            cl.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue(token.TokenType,token.Token);
            HttpResponseMessage result = await cl.GetAsync(service.Url + "config/stop");
            result.EnsureSuccessStatusCode();
        }


        private async Task WaitForStart(string id, int timeout = 10000)
        {
            var tokenSource = new CancellationTokenSource(timeout);
            var token = tokenSource.Token;
            var task = Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (token.IsCancellationRequested)
                        token.ThrowIfCancellationRequested();
                    var servise = this._services[id];
                    if (servise != null && servise.IsRun)
                        break;
                    Thread.Sleep(100);
                }

            }, token);
            await task;
        }


        public async Task<bool> Start(string id, string ver)
        {
            var service = this._services[id];
            if (service != null && service.IsRun)
            {
                try
                {
                    await Stop(id);
                }
                catch
                {
                }
            }
            var param = string.Format("-d \"{0}\" -id {1}", MonitorSrv.RepositoryPath, id);
            if (!string.IsNullOrEmpty(ver))
                param += " -v " + ver;
            
            this._services[id] = new ServiceInfo
            {
                Id = id,
                Version = ver,
                LastActive = DateTime.Now,
                ProcessId = service != null ? service.ProcessId : 0,
                Status = ServiceStatus.Starting,
                
            };

            var process = Process.Start("Wiki.ServiceHost.exe", param);
            try
            {
                await WaitForStart(id);
                service = this._services[id];
                this._logger.WriteEvent(
                    "Strart service host. Service id:{0}, version:{1}, start param:{2}, process id:{3}, url:{4}", id,
                    ver, param, service.ProcessId, service.Url);

                return true;

            }
            catch (OperationCanceledException)
            {
                this._logger.WriteWarning(
                    string.Format(
                        "Service started and not pulling. Service id:{0}, version:{1}, start param:{2},process id:{3}",
                        id, ver, param, process != null ? process.Id + "" : "process is null"));
                return false;

            }
            catch (Exception e)
            {
                this._logger.WriteError(string.Format("Service not started. Service id:{0}, version:{1}, start param:{2}"), e);
                return false;
            }
        }


        public ServiceInfo GetService(string id)
        {
            return this._services[id];
        }

        public ICollection<ServiceInfo> GetServices()
        {
            return this._services.GetAll();
        }

        public bool RegisterService(ServiceInfo info)
        {
            if (info == null)
                return false;
            info.LastActive = DateTime.Now;
            info.Status = ServiceStatus.Run;
            var service = this._services[info.Id];
            if (service == null)
            {
                this._logger.WriteEvent("Register unhandled service. Id:{0}, version:{1},PID:{2}", info.Id, info.Version, info.ProcessId);
            }
            var result = this._services.AddOrRefresh(info);
            return result;
        }
    }
}