using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Hosting;
using CarParts.Common.Log;
using Newtonsoft.Json;
using Wiki.DiscoveryService.Common;
using Wiki.DiscoveryService.Common.DTO;
using Wiki.Service.Common;
using Wiki.Service.Configuration;

namespace Wiki.DiscoveryService
{
    public class MonitorMap
    {
        private const string ActivehostsKey = "ActiveHosts";
        private static MonitorMap _instance;
        private ConcurrentDictionary<string, DiscoveryMonitorNetworkInfo> _monitorDictionary;
        private ServiceConfiguration _configuration;
        private ServiceConfiguration _configurationCache;
        private FileLogger _logger;


        public static MonitorMap Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MonitorMap
                    {
                        _monitorDictionary = new ConcurrentDictionary<string, DiscoveryMonitorNetworkInfo>(),
                        _configuration =
                            new ServiceConfiguration(Constants.ConfigName, HostingEnvironment.MapPath("~/")),
                        _configurationCache =
                            new ServiceConfiguration(Constants.ConfigCacheName, HostingEnvironment.MapPath("~/")),
                        _logger = new FileLogger("discovery")
                    };
                    InitializeMonitorMap();
                }
                return _instance;
            }
        }

        private static void InitializeMonitorMap()
        {
            _instance._configurationCache.Load();
            var cfg = _instance._configurationCache[ActivehostsKey];
            try
            {
                if (!string.IsNullOrWhiteSpace(cfg))
                {
                    var items = JsonConvert.DeserializeObject<DiscoveryMonitorNetworkInfo[]>(cfg);
                    foreach (var item in items)
                    {
                        _instance._monitorDictionary[item.GetKey()] = item;
                    }
                }
            }
            catch (Exception err)
            {
                _instance._logger.WriteError("Error load configuration", err);
            }
        }

        private MonitorMap()
        {

        }

        public bool TryAddMonitor(DiscoveryMonitorNetworkInfo info)
        {
            var result = _monitorDictionary.TryAdd(info.GetKey(), info);
            UpdateConfig();
            return result;
        }

        public void UpdateConfig()
        {
            try
            {
                _configuration[ActivehostsKey] =
                    JsonConvert.SerializeObject(_monitorDictionary.Values.Select(x => new DTONetworkSettings()
                    {
                        RemoteIp = x.RemoteIp,
                        MonitorUrl = x.MonitorUrl,
                        HostName = x.HostName,
                        IsDebug = x.IsDebug,
                        IsDirect = x.isDirect
                    }));
                _configuration.Save();
            }
            catch (Exception ex)
            {
                _logger.WriteError("Update config error", ex);
            }

        }

        public void UpdateConfigCache()
        {
            try
            {
                _configurationCache[ActivehostsKey] = JsonConvert.SerializeObject(_monitorDictionary.Values);
                _configurationCache.Save();
            }
            catch (Exception ex)
            {
                _logger.WriteError("Update config cache error", ex);
            }

        }

        public void AddOrUpdate(DiscoveryMonitorNetworkInfo info)
        {
            _monitorDictionary.AddOrUpdate(info.GetKey(), info, (index, value) => info);
            UpdateConfig();
        }

        public bool TryUpdateMonitor(string key, DiscoveryMonitorNetworkInfo info)
        {
            var result = _monitorDictionary.TryUpdate(key, info, info);
            UpdateConfig();
            return result;
        }

        public bool TryDelete(DTONetworkSettings info, string hostName)
        {
            DiscoveryMonitorNetworkInfo removedElement;
            var result = _monitorDictionary.TryRemove((hostName + info.RemoteIp).ToLower(), out removedElement);
            UpdateConfig();
            UpdateConfigCache();
            return result;
        }

        public int Count()
        {
            return _monitorDictionary.Count;
        }

        public Dictionary<string, DiscoveryMonitorNetworkInfo> ToDictionary()
        {
            return _monitorDictionary.ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public DiscoveryMonitorNetworkInfo GetOneMonitor(string key)
        {
            return _monitorDictionary[key];
        }

        public ConcurrentDictionary<string, DiscoveryMonitorNetworkInfo> GetAllMonitors()
        {
            return _monitorDictionary;
        }
    }
}