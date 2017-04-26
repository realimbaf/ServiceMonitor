using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using CarParts.Common.Log;
using Newtonsoft.Json;
using Wiki.Service.Common;

namespace Wiki.DiscoveryService.Common
{
    public class ListeningService 
    {
        private readonly int _listenPort;
        private readonly FileLogger _logger;

        public ListeningService(int? listenPort = null)
        {
            _listenPort = listenPort ?? 31000;
            _logger = new FileLogger("discovery");
        }
        public async Task<MonitorNetworkInfo> StartListeningAsync()
        {
            var localEp = new IPEndPoint(IPAddress.Any, _listenPort);
            var client = new UdpClient(localEp);
            var multicastaddress = IPAddress.Parse("224.100.10.1");
            client.JoinMulticastGroup(multicastaddress);
            using (client)
            {
                try
                {
                    while (true)
                    {
                        _logger.WriteEvent("Wait broadcast");
                        var response = await client.ReceiveAsync();
                        _logger.WriteEvent("receive broadcast: {0}", response.RemoteEndPoint.Address);

                        var stringContent = Encoding.ASCII.GetString(Convert.FromBase64String(Encoding.ASCII.GetString(response.Buffer)));
                        var contentToJson = JsonConvert.DeserializeObject<DiscoveryMonitorNetworkInfo>(stringContent);
                        contentToJson.MonitorUrl = contentToJson.MonitorUrl.Replace("+", response.RemoteEndPoint.Address.ToString());
                        contentToJson.RemoteIp = response.RemoteEndPoint.Address.ToString();
                        return contentToJson;
                    }
                }
                catch (Exception e)
                {
                    _logger.WriteError("Error broadcast listening", e);
                    return null;
                }
            }
                                         
        }

    }
}