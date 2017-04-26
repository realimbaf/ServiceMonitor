using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CarParts.Common.Log;
using Wiki.Core.Exceptions;
using Wiki.DiscoveryService.Utils;
using Wiki.Service.Common;
using Wiki.Service.Common.Clients;

namespace Wiki.DiscoveryService
{
    public class MonitorWatcher
    {
        private const int INTERVAL = 15000;
        private Timer _timer;
        private readonly IList<Task> _watcherTaskList;
        private readonly FileLogger _logger;

        public MonitorWatcher()
        {
            _watcherTaskList = new List<Task>();
            _logger = new FileLogger("discovery");
        }

        public static MonitorMap MonitorMap
        {
            get { return MonitorMap.Instance; }
        }

        public void Start()
        {
            _logger.WriteEvent("MonitorWatcher start");
            TurnOnTimer();
        }

        public void Stop()
        {
           _logger.WriteEvent("MonitorWatcher stop");
            if (this._timer != null)
            {
                this._timer.Change(-1, -1);
                this._timer = null;
            }
        }

        private void TurnOnTimer()
        {
            if (this._timer == null)
                this._timer = new Timer(CheckMonitorsActivity, null, INTERVAL, Timeout.Infinite);
            else
                this._timer.Change(INTERVAL, Timeout.Infinite);
        }

        public async void CheckMonitorsActivity(object obj)
        {
            try
            {
                if (MonitorMap.Count() != 0)
                {
                    foreach (var monitor in MonitorMap.GetAllMonitors())
                    {
                        if (!monitor.Value.isDirect)
                        {
                            var task = GetMonitorServices(monitor.Value.MonitorUrl, monitor.Value.HostName)
                                .ContinueWith(
                                    t =>
                                    {
                                        if (t.IsCompleted)
                                        {
                                            var monitorInfo = t.Result;
                                            if (monitorInfo != null)
                                            {
                                                UpdateMonitorInfo(monitorInfo, monitor.Value);
                                                _logger.WriteEvent("Info about monitor is updated.");
                                            }
                                            else
                                            {
                                                monitor.Value.IsActive = false;
                                                _logger.WriteEvent("Info about monitor is NOT updated.");
                                            }
                                        }
                                        else
                                        {
                                            _logger.WriteError("Task: update monitor info error", t.Exception);
                                        }
                                    });

                            _watcherTaskList.Add(task);
                        }
                        else if(monitor.Value.isDirect)
                        {
                            if (!CompareDates.IsCompareWithNow(monitor.Value.LastUpdate))
                            {
                                monitor.Value.IsActive = false;
                                MonitorMap.UpdateConfigCache();
                            }
                        }
                    }
                }
                await Task.WhenAll(_watcherTaskList);
            }
            catch (AggregateException ex)
            {
                var exception = ex.Flatten();
                _logger.WriteError("Check monitor task activity error", exception.InnerException);
            }
            catch (Exception ex)
            {
                _logger.WriteError("Check monitor activity error", ex);
            }
            finally
            {
                this.TurnOnTimer();
            }           
        }
        public static void UpdateMonitorInfo(ServiceInfoDiscovery monitorInfo, DiscoveryMonitorNetworkInfo monitor )
        {
            monitor.IsActive = true;
            monitor.LastUpdate = DateTime.Now.ToString("MM/dd/yyyy - HH:mm:ss");
            monitor.Services.Clear();
            monitor.Services.AddRange(monitorInfo.Services);
            monitor.HostName = monitorInfo.HostName;
            monitor.IsDebug = monitorInfo.IsDebug;
            MonitorMap.UpdateConfigCache();
        }
        public async Task<ServiceInfoDiscovery> GetMonitorServices(string monitorUrl,string hostName)
        {       
            try 
            {
                if (monitorUrl != null)
                {                
                    var monitorClient = new ServiceMonitorClient(monitorUrl, "service", "servicepasswd");
                    var result = await monitorClient.GetMonitorServicesAsync();
                    return result;
                }
                return null;
            }
            catch (ArgumentException ex)
            {
                _logger.WriteError(string.Format("Monitor Watcher Error: RemoteIp ({0}) not found", monitorUrl), ex);
                return null;
            }
            catch (WikiApiException ex)
            {
                if (ex.HttpCode == HttpStatusCode.NotFound)
                {
                    var monitorClient = new ServiceMonitorClient(monitorUrl, "service", "servicepasswd");
                    var services =  await monitorClient.GetServicesAsync();
                    return new ServiceInfoDiscovery(false,hostName, monitorUrl)
                    {
                        Services = services.ToList()
                    };
                }
                //todo: updateconfig
                _logger.WriteError("Monitor Watcher Error: RemoteIp not found",ex);
                return null;
            }
            catch (Exception ex)
            {
                _logger.WriteError(string.Format("Monitor Watcher Error: RemoteIp ({0})", monitorUrl), ex);
                return null;
            }
        }

    }
}