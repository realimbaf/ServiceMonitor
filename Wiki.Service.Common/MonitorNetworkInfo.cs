using System;
using System.Collections.Generic;

namespace Wiki.Service.Common
{
    public class MonitorNetworkInfo
    {      
        public string HostName { get; set; }
        public string MonitorUrl { get; set; }
        public bool IsDebug { get; set; }
        public List<string> OptionalIpAdresses { get; set; }
        public string IpAdress { get; set; }

        public MonitorNetworkInfo(string monitorUrl,string hostName = null,  bool isDebug = true, List<string> optionalIPs = null, string ip = null)
        {
            this.HostName = hostName?? Environment.MachineName;
            this.MonitorUrl = monitorUrl;
            this.IsDebug = isDebug;
            this.OptionalIpAdresses = optionalIPs;
            this.IpAdress = ip;
        }
      
    }

    public class DiscoveryMonitorNetworkInfo : MonitorNetworkInfo
    {
        public DiscoveryMonitorNetworkInfo(string monitorUrl,string hostName = null, bool isDebug = true ) : base(monitorUrl,hostName, isDebug)
        {
            Services = new List<ServiceInfo>();
        }
        public string RemoteIp { get; set; }
        public string LastBroadcast { get; set; }
        public string LastUpdate { get; set; }
        public bool IsActive { get; set; }
        public bool isDirect { get; set; }
        public List<ServiceInfo> Services { get; set; }

        public string GetKey()
        {
            return (this.HostName + this.RemoteIp).ToLower();
        }
    }
}
