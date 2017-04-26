using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using CarParts.Common.Log;
using Newtonsoft.Json;
using Wiki.Service.Common;
using Wiki.ServiceMonitor.Utils;

namespace Wiki.ServiceMonitor.MonitorDiscovery
{
    public class MonitorBroadcastStrategy : MonitorRelationStrategy
    {   
        private readonly IDictionary<string, MonitorNetworkInfo> _adapterDictionary;
        private List<string> _optionalIpAdresses;
        private readonly FileLogger _logger;

        public MonitorBroadcastStrategy(int? localport = null) : base(localport)
        { 
            _adapterDictionary = new Dictionary<string, MonitorNetworkInfo>();
            _logger = new FileLogger("mon");
        }

        protected override void SendTick(object obj)
        {
            FillNetworkInformation();                       
            foreach (var adapter in _adapterDictionary)
            {
                using (var client = new UdpClient(new IPEndPoint(IPAddress.Parse(adapter.Value.IpAdress),0)))
                {
                    var multicastaddress = IPAddress.Parse("224.100.10.1");
                    client.JoinMulticastGroup(multicastaddress);
                    var remoteIp = new IPEndPoint(multicastaddress,31000);
                    //var remoteIp = new IPEndPoint(IPAddress.Broadcast, 31000);
                    try
                    {
                        var contentToBase64 =Convert.ToBase64String(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(adapter.Value)));
                        var bytes = Encoding.ASCII.GetBytes(contentToBase64);
                        client.Send(bytes, bytes.Length,
                            remoteIp);
                        _logger.WriteEvent("[Broadcast Strategy] - Send Broadcast.IP broadcast: " + adapter.Key);
                    }
                    catch (Exception ex)
                    {
                        _logger.WriteError("[Broadcast Strategy] - Error: Send message to discovery service is failed.", ex);

                    }
                }
            }

        }

        private void FillNetworkInformation()
        {   
            _optionalIpAdresses = new List<string>();        
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    foreach (var ip in adapter.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            var broadcast = ip.Address.GetBroadcastAddress(ip.IPv4Mask);
                            if (!_adapterDictionary.ContainsKey(broadcast.ToString()))
                            {
                                 AddOptionalIPs(adapter);
                                _adapterDictionary.Add(broadcast.ToString(),
                                    new MonitorNetworkInfo(MonitorSrv.MonitorUrl,null,MonitorSettings.IsDebug, _optionalIpAdresses,ip.Address.ToString()));
                            }
                        }
                    }
                }
            }
        }

        private void AddOptionalIPs(NetworkInterface adapter)
        {
            var optionalIPs = adapter.GetIPProperties().UnicastAddresses
                .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork).ToList();
            if (optionalIPs.Count > 0)
            {
                _optionalIpAdresses.AddRange(optionalIPs.Select(x => x.Address.ToString()));
            }
        }
    }
}
