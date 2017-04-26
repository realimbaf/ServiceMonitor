using System;
using System.Collections.Generic;
using Wiki.Service.Common;

namespace Wiki.DiscoveryService.Common.DTO
{
    public class HostDetail
    {
        public string Hostname { get; set; }
        public string RemoteIp { get; set; }
        public string MonitorUrl { get; set; }
        public string LastBroadcast { get; set; }
        public string LastUpdate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDebug { get; set; }
        public bool IsDirect { get; set; }
    }
    public class DTOMonitors
    {
        public ICollection<ServiceInfo> Services { get; set; }
        public HostDetail HostDetail { get; set; }
      
    }

    public class DTOServices
    {
        public string ServiceName { get; set; }
        public ICollection<HostDetail> Hosts { get; set; }

        public DTOServices()
        {
            Hosts = new List<HostDetail>();
        }

        public void AddHost(HostDetail host)
        {
            this.Hosts.Add(host);
        }
    }
}