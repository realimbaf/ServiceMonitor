using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CarParts.Common.Log;
using Newtonsoft.Json;
using Wiki.Core.Exceptions;
using Wiki.Service.Common;
using Wiki.Service.Common.Clients;
using Wiki.ServiceMonitor.Utils;

namespace Wiki.ServiceMonitor.MonitorDiscovery
{
    public class MonitorDirectRelationStrategy : MonitorRelationStrategy
    {
        private readonly string _discoveryUrl;
        private readonly HttpClient _client;
        private readonly FileLogger _logger;

        public MonitorDirectRelationStrategy(string discoveryUrl)
        {
            _discoveryUrl = discoveryUrl;           
            _client = new HttpClient();
            _logger = new FileLogger("mon");
        }

        protected override async void SendTick(object obj)
        {
            try
            {
                _logger.WriteEvent("[Direct Strategy] - Start.");
                await DirectRelationToDiscovery();
                _logger.WriteEvent("[Direct Strategy] - Monitor relation to Discovery is successful");
            }
            catch (Exception ex)
            {

                _logger.WriteError("[Direct Strategy] - Direct relation to discovery error: ", ex);
            }
        }

        public async Task<ServiceInfoDiscovery> DirectRelationToDiscovery()
        {                     
            try
            {
                var client = new ServiceMonitorClient("http://127.0.0.1:9100", "service", "servicepasswd");
                var services = await client.GetServicesAsync();
                if (services != null)
                {
                    var model = new ServiceInfoDiscovery(MonitorSettings.IsDebug)
                    {
                        Services = services.ToList()
                    };
                    var stringContent = new StringContent(JsonConvert.SerializeObject(model),
                                 Encoding.UTF8,
                                 "application/json");

                    var result = await _client.PostAsync(_discoveryUrl + "/api/hosts/directAdd", stringContent);
                    if (!result.IsSuccessStatusCode)
                    {
                        var message = await result.Content.ReadAsStringAsync();
                        throw new WikiApiException(result.StatusCode, message);
                    }
                    return JsonConvert.DeserializeObject<ServiceInfoDiscovery>(await result.Content.ReadAsStringAsync());
                }
                return null;
            }
            catch (WikiApiException ex)
            {
                _logger.WriteError("Error to send direction discovery api",ex);
                return null;
            }                 
        }
    }
}
