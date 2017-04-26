using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Wiki.Service.Common
{
    public class ServiceInfoBase
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public bool IsRun { get; set; }
    }

    public class ServiceInfo : ServiceInfoBase
    {
        public string Version { get; set; }

        public int ProcessId { get; set; }

        public DateTime LastActive { get; set; }

        public ServiceStatus Status { get; set; }
    }
    public class ServiceInfoExt : ServiceInfo
    {
        public bool IsAuto { get; set; }

        public bool IsManualStop { get; set; }

        public string ActiveVersion { get; set; }
    }

    public class ServiceInfoDiscovery 
    {
        public string HostName { get; }
        public bool IsDebug { get; set; }
        public string RemoteIp { get; }
        public string MonitorUrl { get; set; }
        public IList<ServiceInfo> Services { get; set; }
        public ServiceInfoDiscovery(bool isDebug = true, string hostname = null,string remoteIp = null)
        {
            this.HostName = hostname ?? Environment.MachineName;
            this.IsDebug =  isDebug;
            this.RemoteIp = remoteIp;
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        public string GetKey()
        {
            return (this.HostName + this.RemoteIp).ToLower();
        }
    }
    public enum ServiceStatus
    {
        Stop,
        Stoping,
        Run,
        Starting,
        NotResponse
    }
   
}
