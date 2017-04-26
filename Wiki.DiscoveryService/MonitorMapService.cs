using System;
using System.Collections.Generic;
using System.Linq;
using CarParts.Common.Log;
using Wiki.DiscoveryService.Common.DTO;
using Wiki.Service.Common;

namespace Wiki.DiscoveryService
{
    public class MonitorMapService
    {
        public static MonitorMap MonitorMap
        {
            get { return MonitorMap.Instance; }
        }
        private readonly FileLogger _logger;

        public MonitorMapService()
        {
            _logger = new FileLogger("MonitorMapService");
        }
        public ServiceInfo GetFirstService(string id)
        {
            try
            {
                var item = MonitorMap.GetAllMonitors()
                .Select(monitor => new { Monitor = monitor.Value, Service = monitor.Value.Services.FirstOrDefault(x => x.Id == id && x.IsRun) })
                .FirstOrDefault(x => !x.Monitor.IsDebug && x.Service != null);

                if (item != null)
                {
                    var service = item.Service;
                    return new ServiceInfo()
                    {
                        IsRun = service.IsRun,
                        Id = service.Id,
                        LastActive = service.LastActive,
                        ProcessId = service.ProcessId,
                        Status = service.Status,
                        Version = service.Version,
                        Url = service.Url.Replace(new Uri(service.Url).Host, item.Monitor.RemoteIp)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.WriteError("MonitorCollection - getting the first service. method: GetFirstService error", ex);
                return null;
            }
            return null;
        }

        public Dictionary<string,List<HostDetail>> GetServicesWithGroupByHosts()
        {
            var monitors = MonitorMap.ToDictionary();
            var servicesDictionary = new Dictionary<string, List<HostDetail>>();
            
            monitors.Values.ToList().ForEach((host) =>
            {
                host.Services.Aggregate(servicesDictionary, (summ, next) =>
                {
                    Iterator(summ, next, host);
                    return summ;
                });
            });
            return servicesDictionary;
        }

        private static void Iterator(IDictionary<string, List<HostDetail>> summ, ServiceInfoBase next, DiscoveryMonitorNetworkInfo host)
        {
            if (summ.ContainsKey(next.Id))
            {
                summ[next.Id].Add(new HostDetail()
                {
                    RemoteIp = host.RemoteIp,
                    MonitorUrl = host.MonitorUrl,
                    IsDebug = host.IsDebug,
                    IsActive = host.IsActive,
                    Hostname = host.HostName,
                    LastBroadcast = host.LastBroadcast,
                    LastUpdate = host.LastUpdate,
                    IsDirect = host.isDirect
                });
            }
            else
            {
                summ.Add(next.Id, new List<HostDetail>
                {
                    new HostDetail()
                    {
                        RemoteIp = host.RemoteIp,
                        MonitorUrl = host.MonitorUrl,
                        IsDebug = host.IsDebug,
                        IsActive = host.IsActive,
                        Hostname = host.HostName,
                        LastBroadcast = host.LastBroadcast,
                        LastUpdate = host.LastUpdate,
                        IsDirect = host.isDirect
                    }
                });
            }
        }

        public bool RunService(string id)
        {
            return true;
        }
    }
}