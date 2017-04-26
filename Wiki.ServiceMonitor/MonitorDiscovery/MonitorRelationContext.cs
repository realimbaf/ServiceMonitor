using System;
using System.Diagnostics;
using CarParts.Common.Log;
using Wiki.ServiceMonitor.Utils;

namespace Wiki.ServiceMonitor.MonitorDiscovery
{
    public class MonitorRelationContext
    {
        private readonly MonitorRelationStrategy _strategy;
        private readonly FileLogger _logger;

        public MonitorRelationContext()
        {
            _strategy = CalculateStrategy();
            _logger = new FileLogger("mon");
        }

        public void Start()
        {
            _strategy.Start();
        }

        public void Stop()
        {
            _strategy.Stop();
        }

        private MonitorRelationStrategy CalculateStrategy()
        {
            try
            {
                if (MonitorSettings.EnableBroadcast)
                {
                    return new MonitorBroadcastStrategy();
                }
                if (MonitorSettings.isExistDiscoveryUrl)
                {
                    return new MonitorDirectRelationStrategy(MonitorSettings.GetSetting(MonitorSettings.DiscoveryUrl));
                }
                throw new InvalidOperationException();
            }
            catch (Exception ex)
            {
               _logger.WriteError("Calculate strategy error. Check you ServiceMonitor app settings file.",ex);
                return new MonitorBroadcastStrategy();
            }
           
        }
    }
}
